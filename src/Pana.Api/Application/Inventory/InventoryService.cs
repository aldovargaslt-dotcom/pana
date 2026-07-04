using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Inventory;

public interface IInventoryService
{
    Task<InventoryMovementDto> StockInAsync(StockInRequest request, Guid? userId = null, CancellationToken ct = default);
    Task<InventoryMovementDto> StockOutAsync(StockOutRequest request, Guid? userId = null, CancellationToken ct = default);
    Task<InventoryMovementDto> AdjustAsync(AdjustmentRequest request, Guid? userId = null, CancellationToken ct = default);
    Task<List<StockLevelDto>> GetStockLevelsAsync(CancellationToken ct = default);
    Task<List<InventoryMovementDto>> GetMovementsAsync(Guid? productId = null, int limit = 50, CancellationToken ct = default);
    Task DeductForSaleAsync(Guid productId, decimal quantity, Guid saleId, CancellationToken ct = default);
}

public class InventoryService : IInventoryService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public InventoryService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<InventoryMovementDto> StockInAsync(StockInRequest request, Guid? userId = null, CancellationToken ct = default)
    {
        var movement = new InventoryMovement(
            _tenantContext.TenantId,
            request.ProductId,
            InventoryMovement.Types.StockIn,
            request.Quantity,
            request.Reason,
            performedByUserId: userId);

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);
        return MapToDto(movement);
    }

    public async Task<InventoryMovementDto> StockOutAsync(StockOutRequest request, Guid? userId = null, CancellationToken ct = default)
    {
        var movement = new InventoryMovement(
            _tenantContext.TenantId,
            request.ProductId,
            InventoryMovement.Types.StockOut,
            request.Quantity,
            request.Reason,
            performedByUserId: userId);

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);
        return MapToDto(movement);
    }

    public async Task<InventoryMovementDto> AdjustAsync(AdjustmentRequest request, Guid? userId = null, CancellationToken ct = default)
    {
        // Calculate the adjustment delta
        var currentStock = await GetCurrentStockForProduct(request.ProductId, ct);
        var delta = request.NewStockLevel - currentStock;

        var movement = new InventoryMovement(
            _tenantContext.TenantId,
            request.ProductId,
            InventoryMovement.Types.Adjustment,
            delta,
            request.Reason ?? "Manual stock adjustment",
            performedByUserId: userId);

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);
        return MapToDto(movement);
    }

    public async Task DeductForSaleAsync(Guid productId, decimal quantity, Guid saleId, CancellationToken ct = default)
    {
        var movement = new InventoryMovement(
            _tenantContext.TenantId,
            productId,
            InventoryMovement.Types.SaleDeduction,
            quantity,
            referenceSaleId: saleId);

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<StockLevelDto>> GetStockLevelsAsync(CancellationToken ct = default)
    {
        var stockSums = await _db.InventoryMovements
            .GroupBy(m => m.ProductId)
            .Select(g => new { ProductId = g.Key, CurrentStock = g.Sum(m => m.Quantity) })
            .ToListAsync(ct);

        var productIds = stockSums.Select(s => s.ProductId).ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return stockSums
            .Where(s => products.ContainsKey(s.ProductId))
            .Select(s =>
            {
                var p = products[s.ProductId];
                return new StockLevelDto(s.ProductId, p.Name, p.Sku, s.CurrentStock, p.Cost);
            })
            .OrderBy(s => s.ProductName)
            .ToList();
    }

    public async Task<List<InventoryMovementDto>> GetMovementsAsync(Guid? productId = null, int limit = 50, CancellationToken ct = default)
    {
        var query = _db.InventoryMovements.AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => MapToDto(m))
            .ToListAsync(ct);
    }

    private async Task<decimal> GetCurrentStockForProduct(Guid productId, CancellationToken ct)
    {
        return await _db.InventoryMovements
            .Where(m => m.ProductId == productId)
            .SumAsync(m => m.Quantity, ct);
    }

    private static InventoryMovementDto MapToDto(InventoryMovement m) => new(
        m.Id,
        m.ProductId,
        m.MovementType,
        m.Quantity,
        m.Reason,
        m.ReferenceSaleId,
        m.SourceLocationId,
        m.DestinationLocationId,
        m.CreatedAt
    );
}
