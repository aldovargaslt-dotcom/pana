using Microsoft.EntityFrameworkCore;
using Pana.Api.Application.Common;
using Pana.Api.Domain.Products;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Products;

public interface IProductService
{
    Task<PagedList<ProductDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<ProductDto>> GetAllAsync(CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class ProductService : IProductService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProductService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Products
            .OrderBy(p => p.Name)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);
    }

    public async Task<PagedList<ProductDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Products.OrderBy(p => p.Name);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);
        return new PagedList<ProductDto>(items, totalCount, page, pageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(ProductRequest request, CancellationToken ct = default)
    {
        var product = new Product(
            _tenantContext.TenantId,
            request.Name,
            request.Sku,
            request.Price,
            request.Cost
        );

        if (request.Description is not null)
            product.SetDescription(request.Description);

        if (request.ProductType is not null)
            product.SetProductType(request.ProductType);

        if (request.UnitOfMeasureId.HasValue)
            product.SetUnitOfMeasure(request.UnitOfMeasureId);

        if (request.PurchaseUnitOfMeasureId.HasValue)
            product.SetPurchaseUnitOfMeasure(request.PurchaseUnitOfMeasureId);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, ProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct);
        if (product is null) return null;

        product.SetName(request.Name);
        product.SetSku(request.Sku);
        product.SetPricing(request.Price, request.Cost);
        product.SetDescription(request.Description);

        if (request.ProductType is not null)
            product.SetProductType(request.ProductType);

        product.SetUnitOfMeasure(request.UnitOfMeasureId);
        product.SetPurchaseUnitOfMeasure(request.PurchaseUnitOfMeasureId);

        await _db.SaveChangesAsync(ct);
        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct);
        if (product is null) return false;

        product.Deactivate(); // Soft delete — never hard delete business data
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Sku,
        p.Price,
        p.Cost,
        p.Margin,
        p.MarginPercentage,
        p.IsActive,
        p.ProductType,
        p.UnitOfMeasureId,
        p.PurchaseUnitOfMeasureId,
        p.CreatedAt,
        p.UpdatedAt
    );
}
