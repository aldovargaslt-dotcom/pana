using Microsoft.EntityFrameworkCore;
using Pana.Api.Application.Common;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Sales;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Sales;

public interface ISalesService
{
    Task<PagedList<SaleDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<SaleDto>> GetAllAsync(CancellationToken ct);
    Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SaleDto> CreateAsync(CreateSaleRequest request, Guid? soldByUserId = null, CancellationToken ct = default);
    Task<bool> ConfirmAsync(Guid id, CancellationToken ct = default);
    Task<bool> StartProductionAsync(Guid id, CancellationToken ct = default);
    Task<bool> MarkReadyAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeliverAsync(Guid id, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    Task<bool> RecordPaymentAsync(Guid id, decimal amount, CancellationToken ct = default);
    Task<bool> UpdatePreOrderDetailsAsync(Guid id, UpdatePreOrderRequest request, CancellationToken ct = default);
    Task<DailySalesSummary> GetDailySummaryAsync(DateTime? date = null, CancellationToken ct = default);
    Task<List<SaleDto>> GetPreOrdersAsync(CancellationToken ct = default);
}

public class SalesService : ISalesService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly DomainEventDispatcher _eventDispatcher;

    public SalesService(PanaDbContext db, ITenantContext tenantContext, DomainEventDispatcher eventDispatcher)
    {
        _db = db;
        _tenantContext = tenantContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<List<SaleDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToDto(s))
            .ToListAsync(ct);
    }

    public async Task<PagedList<SaleDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync(ct);
        return new PagedList<SaleDto>(items, totalCount, page, pageSize);
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return sale is null ? null : MapToDto(sale);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, Guid? soldByUserId = null, CancellationToken ct = default)
    {
        // Load product names for snapshot
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // Sale starts as Draft
        var sale = new Sale(
            _tenantContext.TenantId,
            soldByUserId,
            request.Notes,
            request.OrderType,
            request.CustomerName,
            request.CustomerPhone,
            request.ScheduledDate,
            request.DepositAmount,
            request.PaymentMethod,
            request.InternalNotes
        );

        foreach (var item in request.Items)
        {
            var productName = products.TryGetValue(item.ProductId, out var p)
                ? p.Name
                : "Unknown Product";

            sale.AddItem(item.ProductId, productName, item.UnitPrice, item.Quantity);
        }

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(ct);

        return MapToDto(sale);
    }

    public async Task<bool> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sale is null) return false;

        sale.Confirm();

        // Standard sales: auto-complete and deduct inventory immediately
        if (sale.OrderType == Sale.OrderTypes.Standard)
        {
            sale.Complete();
            await _db.SaveChangesAsync(ct);

            var items = sale.Items
                .Where(i => !i.IsVoided)
                .Select(i => new SaleItemSnapshot(i.ProductId, i.Quantity))
                .ToList();
            await _eventDispatcher.PublishAsync(
                new SaleCompletedEvent(sale.Id, sale.TenantId, items), ct);

            return true;
        }

        // Pre-orders: just confirm, inventory is deducted later on Deliver
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> StartProductionAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FindAsync([id], ct);
        if (sale is null) return false;

        sale.StartProduction();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkReadyAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FindAsync([id], ct);
        if (sale is null) return false;

        sale.MarkReady();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeliverAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (sale is null) return false;

        sale.Deliver();
        await _db.SaveChangesAsync(ct);

        // Fire domain event for inventory deduction (only when delivered)
        var items = sale.Items.Select(i =>
            new SaleItemSnapshot(i.ProductId, Math.Abs(i.Quantity))).ToList();
        await _eventDispatcher.PublishAsync(new SaleCompletedEvent(sale.Id, sale.TenantId, items), ct);

        return true;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FindAsync([id], ct);
        if (sale is null) return false;

        sale.Cancel();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RecordPaymentAsync(Guid id, decimal amount, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FindAsync([id], ct);
        if (sale is null) return false;

        sale.RecordPayment(amount);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdatePreOrderDetailsAsync(Guid id, UpdatePreOrderRequest request, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FindAsync([id], ct);
        if (sale is null) return false;

        sale.SetPreOrderDetails(
            request.CustomerName ?? sale.CustomerName ?? "",
            request.CustomerPhone ?? sale.CustomerPhone,
            request.ScheduledDate ?? sale.ScheduledDate ?? DateTime.UtcNow,
            request.DepositAmount ?? sale.DepositAmount,
            request.InternalNotes ?? sale.InternalNotes
        );
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<DailySalesSummary> GetDailySummaryAsync(DateTime? date = null, CancellationToken ct = default)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var nextDate = targetDate.AddDays(1);

        var sales = await _db.Sales
            .Include(s => s.Items)
            .Where(s => s.CreatedAt >= targetDate && s.CreatedAt < nextDate
                     && (s.Status == Sale.Statuses.Delivered || s.Status == Sale.Statuses.Completed))
            .ToListAsync(ct);

        var totalRevenue = sales.Sum(s => s.TotalAmount);

        // Calculate margin: for each item, find product cost
        var productIds = sales.SelectMany(s => s.Items).Select(i => i.ProductId).Distinct();
        var productCosts = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Cost, ct);

        var totalCost = sales.Sum(s =>
            s.Items.Sum(i =>
                productCosts.TryGetValue(i.ProductId, out var cost) ? cost * i.Quantity : 0));

        return new DailySalesSummary(
            targetDate,
            sales.Count,
            totalRevenue,
            totalRevenue - totalCost
        );
    }

    public async Task<List<SaleDto>> GetPreOrdersAsync(CancellationToken ct = default)
    {
        return await _db.Sales
            .Include(s => s.Items)
            .Where(s => s.OrderType == Sale.OrderTypes.PreOrder
                && s.Status != Sale.Statuses.Cancelled
                && s.Status != Sale.Statuses.Delivered)
            .OrderBy(s => s.ScheduledDate)
            .Select(s => MapToDto(s))
            .ToListAsync(ct);
    }

    private static SaleDto MapToDto(Sale s) => new(
        s.Id,
        s.Status,
        s.TotalAmount,
        s.Notes,
        s.SoldByUserId,
        s.CreatedAt,
        s.Items.Select(i => new SaleItemDto(
            i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal, i.IsVoided
        )).ToList(),
        s.OrderType,
        s.CustomerName,
        s.CustomerPhone,
        s.ScheduledDate,
        s.DepositAmount,
        s.BalanceDue,
        s.PaymentStatus,
        s.PaymentMethod,
        s.InternalNotes
    );
}
