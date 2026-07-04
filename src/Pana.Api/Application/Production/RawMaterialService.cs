using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Production;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Production;

public interface IRawMaterialService
{
    Task<List<RawMaterialDto>> GetAllAsync(string? category = null, CancellationToken ct = default);
    Task<RawMaterialDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RawMaterialDto> CreateAsync(RawMaterialRequest request, CancellationToken ct = default);
    Task<RawMaterialDto?> UpdateAsync(Guid id, RawMaterialRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class RawMaterialService : IRawMaterialService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public RawMaterialService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<RawMaterialDto>> GetAllAsync(string? category = null, CancellationToken ct = default)
    {
        var query = _db.RawMaterials.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        return await query
            .Where(m => m.IsActive)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .Select(m => MapToDto(m))
            .ToListAsync(ct);
    }

    public async Task<RawMaterialDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var material = await _db.RawMaterials.FindAsync([id], ct);
        return material is null ? null : MapToDto(material);
    }

    public async Task<RawMaterialDto> CreateAsync(RawMaterialRequest request, CancellationToken ct = default)
    {
        var material = new RawMaterial(
            _tenantContext.TenantId,
            request.Name,
            request.Category,
            request.PurchaseUnit,
            request.PurchasePrice,
            request.PresentationQty,
            request.BaseUnit,
            request.YieldPct,
            request.Supplier);

        _db.RawMaterials.Add(material);
        await _db.SaveChangesAsync(ct);

        return MapToDto(material);
    }

    public async Task<RawMaterialDto?> UpdateAsync(Guid id, RawMaterialRequest request, CancellationToken ct = default)
    {
        var material = await _db.RawMaterials.FindAsync([id], ct);
        if (material is null) return null;

        material.SetName(request.Name);
        material.SetCategory(request.Category);
        material.SetPurchaseInfo(request.PurchaseUnit, request.PurchasePrice, request.PresentationQty, request.BaseUnit);
        material.SetYield(request.YieldPct);
        material.SetSupplier(request.Supplier);

        await _db.SaveChangesAsync(ct);
        return MapToDto(material);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var material = await _db.RawMaterials.FindAsync([id], ct);
        if (material is null) return false;

        material.Deactivate();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static RawMaterialDto MapToDto(RawMaterial m) => new(
        m.Id,
        m.Name,
        m.Category,
        m.PurchaseUnit,
        m.PurchasePrice,
        m.YieldPct,
        m.PresentationQty,
        m.BaseUnit,
        m.Supplier,
        m.IsActive,
        m.CostPerBaseUnit,
        m.CreatedAt,
        m.UpdatedAt
    );
}
