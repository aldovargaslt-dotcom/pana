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
            .FirstOrDefaultAsync(d => d.Date == date.Date, ct);

        return production is null ? null : MapToDto(production);
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

    public async Task<DailyProductionDto> CloseTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var production = await _db.DailyProductions
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Date == today, ct)
            ?? throw new InvalidOperationException("No production capture found for today. Create one first.");

        if (production.IsClosed)
            throw new InvalidOperationException("Today's production is already closed.");

        // Create InventoryMovements for each line
        foreach (var line in production.Lines)
        {
            // Production → StockIn
            if (line.Produccion > 0)
            {
                _db.InventoryMovements.Add(new InventoryMovement(
                    _tenantContext.TenantId,
                    line.ProductId,
                    InventoryMovement.Types.ProductionIn,
                    line.Produccion,
                    reason: $"Daily production {today:yyyy-MM-dd}",
                    performedByUserId: userId));
            }

            // Returns/Waste → StockOut
            if (line.Devolucion > 0)
            {
                _db.InventoryMovements.Add(new InventoryMovement(
                    _tenantContext.TenantId,
                    line.ProductId,
                    InventoryMovement.Types.ProductionOut,
                    line.Devolucion,
                    reason: $"Daily waste {today:yyyy-MM-dd}",
                    performedByUserId: userId));
            }
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

    private static DailyProductionDto MapToDto(DailyProduction p) => new(
        p.Id,
        p.DailyContextId,
        p.Date,
        p.IsClosed,
        p.ClosedAt,
        p.Lines.Select(l => new DailyProductionLineDto(
            l.Id, l.ProductId, l.ProductName, l.Inicial, l.Produccion, l.Devolucion, l.Disponible
        )).ToList(),
        p.CreatedAt,
        p.UpdatedAt
    );
}
