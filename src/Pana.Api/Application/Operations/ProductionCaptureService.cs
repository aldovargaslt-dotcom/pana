using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Inventory;
using Pana.Api.Domain.Operations;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Operations;

public interface IProductionCaptureService
{
    Task<DailyProductionDto?> GetTodayAsync(CancellationToken ct = default);
    Task<DailyProductionDto?> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<DailyProductionDto> UpsertTodayAsync(UpsertDailyProductionRequest request, CancellationToken ct = default);
    Task<ProductionEventDto> AddEventAsync(AddProductionEventRequest request, Guid userId, CancellationToken ct = default);
    Task<DailyProductionDto> CloseTodayAsync(Guid userId, CancellationToken ct = default);
    Task ReopenTodayAsync(CancellationToken ct = default);
}

public class ProductionCaptureService : IProductionCaptureService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProductionCaptureService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<DailyProductionDto?> GetTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await GetByDateAsync(today, ct);
    }

    public async Task<DailyProductionDto?> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var production = await _db.DailyProductions
            .Include(d => d.Lines)
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.Date == date.Date, ct);

        if (production is null) return null;

        var userIds = production.Events.Select(e => e.RegisteredByUserId).Distinct().ToList();
        var userNames = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        return MapToDto(production, userNames);
    }

    public async Task<DailyProductionDto> UpsertTodayAsync(UpsertDailyProductionRequest request, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Ensure a DailyContext exists for today
        var context = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (context is null)
        {
            context = new DailyContext(_tenantContext.TenantId, today);
            _db.DailyContexts.Add(context);
            await _db.SaveChangesAsync(ct);
        }

        // Find or create today's production capture
        var production = await _db.DailyProductions
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (production is null)
        {
            production = new DailyProduction(_tenantContext.TenantId, context.Id, today);
            _db.DailyProductions.Add(production);
        }

        if (production.IsClosed)
            throw new InvalidOperationException("Today's production is already closed. Reopen it first.");

        // Upsert each line
        foreach (var line in request.Lines)
        {
            production.AddOrUpdateLine(line.ProductId, line.ProductName, line.Inicial, line.Produccion, line.Devolucion);
        }

        await _db.SaveChangesAsync(ct);
        return MapToDto(production);
    }

    public async Task<ProductionEventDto> AddEventAsync(AddProductionEventRequest request, Guid userId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Ensure a DailyContext exists for today
        var context = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (context is null)
        {
            context = new DailyContext(_tenantContext.TenantId, today);
            _db.DailyContexts.Add(context);
            await _db.SaveChangesAsync(ct);
        }

        // Find or create today's production capture
        var production = await _db.DailyProductions
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (production is null)
        {
            production = new DailyProduction(_tenantContext.TenantId, context.Id, today);
            _db.DailyProductions.Add(production);
        }

        if (production.IsClosed)
            throw new InvalidOperationException("Today's production is already closed. Reopen it first.");

        // Inicial can only be set once per product per day
        if (request.EventType == ProductionEvent.Types.Inicial)
        {
            var existingInicial = production.Events
                .Any(e => e.ProductId == request.ProductId && e.EventType == ProductionEvent.Types.Inicial);

            if (existingInicial)
                throw new InvalidOperationException($"Initial count for this product is already set. Adjust it instead.");
        }

        var evt = production.AddEvent(
            request.ProductId,
            request.ProductName,
            request.EventType,
            request.Quantity,
            userId,
            request.Notes);

        await _db.SaveChangesAsync(ct);

        // Get user name
        var userName = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(ct) ?? "Unknown";

        return MapEventToDto(evt, userName);
    }

    public async Task<DailyProductionDto> CloseTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var production = await _db.DailyProductions
            .Include(d => d.Lines)
            .Include(d => d.Events)
            .FirstOrDefaultAsync(d => d.Date == today, ct)
            ?? throw new InvalidOperationException("No production capture found for today. Create one first.");

        if (production.IsClosed)
            throw new InvalidOperationException("Today's production is already closed.");

        // ── Build aggregates from events ──────────────────────
        // Group events by product, then sum by type
        var productAggregates = production.Events
            .GroupBy(e => e.ProductId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Produccion = g.Where(e => e.EventType == ProductionEvent.Types.Produccion).Sum(e => e.Quantity),
                    Devolucion = g.Where(e => e.EventType == ProductionEvent.Types.Devolucion).Sum(e => e.Quantity),
                    ProductName = g.First().ProductName
                });

        // Create InventoryMovements from event aggregates
        foreach (var (productId, agg) in productAggregates)
        {
            if (agg.Produccion > 0)
            {
                _db.InventoryMovements.Add(new InventoryMovement(
                    _tenantContext.TenantId,
                    productId,
                    InventoryMovement.Types.ProductionIn,
                    agg.Produccion,
                    reason: $"Daily production {today:yyyy-MM-dd}",
                    performedByUserId: userId));
            }

            if (agg.Devolucion > 0)
            {
                _db.InventoryMovements.Add(new InventoryMovement(
                    _tenantContext.TenantId,
                    productId,
                    InventoryMovement.Types.ProductionOut,
                    agg.Devolucion,
                    reason: $"Daily waste {today:yyyy-MM-dd}",
                    performedByUserId: userId));
            }

            // Also update legacy DailyProductionLine for backward compat
            production.AddOrUpdateLine(productId, agg.ProductName, 0, agg.Produccion, agg.Devolucion);
        }

        // Close the daily context too
        var context = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Id == production.DailyContextId, ct);
        context?.CloseDay();

        production.Close(userId);
        await _db.SaveChangesAsync(ct);

        return MapToDto(production);
    }

    public async Task ReopenTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var production = await _db.DailyProductions
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (production is not null && production.IsClosed)
        {
            production.Reopen();
            await _db.SaveChangesAsync(ct);
        }
    }

    private static DailyProductionDto MapToDto(DailyProduction p, Dictionary<Guid, string>? userNames = null) => new(
        p.Id,
        p.DailyContextId,
        p.Date,
        p.IsClosed,
        p.ClosedAt,
        p.Lines.Select(l => new DailyProductionLineDto(
            l.Id, l.ProductId, l.ProductName, l.Inicial, l.Produccion, l.Devolucion, l.Disponible
        )).ToList(),
        p.Events.Select(e =>
        {
            var userName = userNames?.GetValueOrDefault(e.RegisteredByUserId) ?? "Unknown";
            return MapEventToDto(e, userName);
        }).ToList(),
        p.CreatedAt,
        p.UpdatedAt
    );

    private static ProductionEventDto MapEventToDto(ProductionEvent e, string userName) => new(
        e.Id,
        e.DailyProductionId,
        e.ProductId,
        e.ProductName,
        e.EventType,
        e.Quantity,
        e.Notes,
        e.RegisteredByUserId,
        userName,
        e.CreatedAt
    );
}
