using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Inventory;

public interface IWasteCategoryService
{
    Task<List<WasteCategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<WasteCategoryDto> CreateAsync(WasteCategoryCreateRequest request, CancellationToken ct = default);
    Task<WasteCategoryDto?> UpdateAsync(Guid id, WasteCategoryUpdateRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<WasteCategoryDto>> SeedDefaultsAsync(CancellationToken ct = default);
}

public class WasteCategoryService : IWasteCategoryService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public WasteCategoryService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<WasteCategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.WasteCategories
            .Where(w => w.IsActive)
            .OrderBy(w => w.SortOrder)
            .Select(w => MapToDto(w))
            .ToListAsync(ct);
    }

    public async Task<WasteCategoryDto> CreateAsync(WasteCategoryCreateRequest request, CancellationToken ct = default)
    {
        var category = new WasteCategory(
            _tenantContext.TenantId,
            request.Name,
            request.SortOrder,
            request.Description);

        if (request.Subcategories is not null)
        {
            foreach (var subName in request.Subcategories)
                category.AddSubcategory(subName);
        }

        _db.WasteCategories.Add(category);
        await _db.SaveChangesAsync(ct);
        return MapToDto(category);
    }

    public async Task<WasteCategoryDto?> UpdateAsync(Guid id, WasteCategoryUpdateRequest request, CancellationToken ct = default)
    {
        var category = await _db.WasteCategories
            .Include(w => w.Subcategories)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

        if (category is null) return null;

        if (request.Name is not null) category.SetName(request.Name);
        if (request.Description is not null) category.SetDescription(request.Description);
        if (request.SortOrder.HasValue) category.SetSortOrder(request.SortOrder.Value);

        await _db.SaveChangesAsync(ct);
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _db.WasteCategories.FindAsync(new object[] { id }, ct);
        if (category is null) return false;

        category.Deactivate();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>
    /// Seeds the default waste categories based on real bakery operational knowledge.
    /// Only creates categories that don't already exist for the current tenant.
    /// </summary>
    public async Task<List<WasteCategoryDto>> SeedDefaultsAsync(CancellationToken ct = default)
    {
        var existing = await _db.WasteCategories
            .Where(w => w.IsActive)
            .Select(w => w.Name)
            .ToListAsync(ct);

        var defaults = new (string Name, string? Description, string[] Subcategories, int SortOrder)[]
        {
            ("Quemado",       "Productos dañados por exceso de cocción",    new[] { "Horno", "Descuido" }, 1),
            ("Deformado",     "Productos con defectos de forma",            new[] { "Masa", "Manipulación" }, 2),
            ("Sobrante",      "Excedente de producción no vendido",         new[] { "Sobreproducción", "Baja demanda" }, 3),
            ("Caducidad",     "Productos vencidos o pasados",               new[] { "Tiempo en vitrina" }, 4),
            ("Manipulación",  "Daños por manejo o transporte",              new[] { "Caída", "Transporte" }, 5),
            ("Calidad",       "No cumple estándar de calidad",              new[] { "Sabor", "Textura", "Apariencia" }, 6),
            ("Otro",          "Otras causas no categorizadas",              Array.Empty<string>(), 99),
        };

        var created = new List<WasteCategory>();

        foreach (var (name, description, subcategories, sortOrder) in defaults)
        {
            if (existing.Contains(name)) continue;

            var category = new WasteCategory(_tenantContext.TenantId, name, sortOrder, description);
            foreach (var sub in subcategories)
                category.AddSubcategory(sub);

            _db.WasteCategories.Add(category);
            created.Add(category);
        }

        if (created.Count > 0)
            await _db.SaveChangesAsync(ct);

        return created.Select(MapToDto).ToList();
    }

    private static WasteCategoryDto MapToDto(WasteCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive,
        Subcategories = entity.Subcategories
            .Select(s => new WasteSubcategoryDto { Id = s.Id, Name = s.Name })
            .ToList(),
        CreatedAt = entity.CreatedAt,
    };
}
