using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Production;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Production;

public interface IRecipeService
{
    Task<List<RecipeDto>> GetAllAsync(CancellationToken ct = default);
    Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RecipeDto> CreateAsync(CreateRecipeRequest request, CancellationToken ct = default);
    Task<RecipeDto?> UpdateAsync(Guid id, UpdateRecipeRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<RecipeDto?> SetIngredientsAsync(Guid recipeId, SetRecipeIngredientsRequest request, CancellationToken ct = default);
    Task<RecipeDto?> RecalculateCostsAsync(Guid recipeId, CancellationToken ct = default);
}

public class RecipeService : IRecipeService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public RecipeService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<RecipeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var recipes = await _db.Recipes
            .Where(r => r.IsActive)
            .Include(r => r.Ingredients)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        var materialIds = recipes
            .SelectMany(r => r.Ingredients)
            .Select(i => i.MaterialId)
            .Distinct()
            .ToList();

        var materials = await _db.RawMaterials
            .Where(m => materialIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        return recipes.Select(r => MapToDto(r, materials)).ToList();
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recipe is null) return null;

        var materialIds = recipe.Ingredients.Select(i => i.MaterialId).Distinct().ToList();
        var materials = await _db.RawMaterials
            .Where(m => materialIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        return MapToDto(recipe, materials);
    }

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request, CancellationToken ct = default)
    {
        var recipe = new Recipe(
            _tenantContext.TenantId,
            request.Name,
            request.Yield,
            request.YieldUnit,
            request.LaborCostPerUnit,
            request.EnergyCost,
            request.OverheadPct,
            request.ProductId);

        if (request.Ingredients is { Count: > 0 })
        {
            // Load materials to calculate costs
            var materialIds = request.Ingredients.Select(i => i.MaterialId).Distinct().ToList();
            var materials = await _db.RawMaterials
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, ct);

            foreach (var ing in request.Ingredients)
            {
                var cost = 0m;
                if (materials.TryGetValue(ing.MaterialId, out var material))
                {
                    cost = UnitConversion.CalculateIngredientCost(
                        ing.Qty,
                        ing.Unit,
                        material.BaseUnit,
                        material.CostPerBaseUnit);
                }
                recipe.AddIngredient(ing.MaterialId, ing.Qty, ing.Unit, cost);
            }
        }

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(ct);

        return MapToDto(recipe, await LoadMaterialsForRecipeAsync(recipe, ct));
    }

    public async Task<RecipeDto?> UpdateAsync(Guid id, UpdateRecipeRequest request, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recipe is null) return null;

        if (request.Name is not null) recipe.SetName(request.Name);
        if (request.Yield.HasValue && request.YieldUnit is not null)
            recipe.SetYield(request.Yield.Value, request.YieldUnit);
        else if (request.YieldUnit is not null && request.Yield.HasValue)
            recipe.SetYield(request.Yield.Value, request.YieldUnit);
        if (request.LaborCostPerUnit.HasValue) recipe.SetLaborCost(request.LaborCostPerUnit.Value);
        if (request.EnergyCost.HasValue) recipe.SetEnergyCost(request.EnergyCost.Value);
        if (request.OverheadPct.HasValue) recipe.SetOverhead(request.OverheadPct.Value);
        recipe.SetProduct(request.ProductId);

        await _db.SaveChangesAsync(ct);

        var materials = await LoadMaterialsForRecipeAsync(recipe, ct);
        return MapToDto(recipe, materials);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes.FindAsync([id], ct);
        if (recipe is null) return false;

        recipe.Deactivate();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<RecipeDto?> SetIngredientsAsync(Guid recipeId, SetRecipeIngredientsRequest request, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);

        if (recipe is null) return null;

        // Clear existing ingredients
        recipe.ClearIngredients();

        if (request.Ingredients.Count > 0)
        {
            var materialIds = request.Ingredients.Select(i => i.MaterialId).Distinct().ToList();
            var materials = await _db.RawMaterials
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, ct);

            foreach (var ing in request.Ingredients)
            {
                var cost = 0m;
                if (materials.TryGetValue(ing.MaterialId, out var material))
                {
                    cost = UnitConversion.CalculateIngredientCost(
                        ing.Qty,
                        ing.Unit,
                        material.BaseUnit,
                        material.CostPerBaseUnit);
                }
                recipe.AddIngredient(ing.MaterialId, ing.Qty, ing.Unit, cost);
            }
        }

        await _db.SaveChangesAsync(ct);

        var loadedMaterials = await LoadMaterialsForRecipeAsync(recipe, ct);
        return MapToDto(recipe, loadedMaterials);
    }

    public async Task<RecipeDto?> RecalculateCostsAsync(Guid recipeId, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct);

        if (recipe is null) return null;

        var materialIds = recipe.Ingredients.Select(i => i.MaterialId).Distinct().ToList();
        var materials = await _db.RawMaterials
            .Where(m => materialIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        recipe.RecalculateIngredientCosts(materialId =>
        {
            if (materials.TryGetValue(materialId, out var m))
                return m.CostPerBaseUnit;
            return 0;
        });

        await _db.SaveChangesAsync(ct);
        return MapToDto(recipe, materials);
    }

    // ── Private helpers ────────────────────────────────────────

    private async Task<Dictionary<Guid, RawMaterial>> LoadMaterialsForRecipeAsync(Recipe recipe, CancellationToken ct)
    {
        var materialIds = recipe.Ingredients.Select(i => i.MaterialId).Distinct().ToList();
        if (materialIds.Count == 0) return new Dictionary<Guid, RawMaterial>();

        return await _db.RawMaterials
            .Where(m => materialIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);
    }

    private static RecipeDto MapToDto(Recipe r, Dictionary<Guid, RawMaterial> materials) => new(
        r.Id,
        r.Name,
        r.Yield,
        r.YieldUnit,
        r.LaborCostPerUnit,
        r.EnergyCost,
        r.OverheadPct,
        r.IsActive,
        r.ProductId,
        r.Ingredients.Select(i =>
        {
            materials.TryGetValue(i.MaterialId, out var m);
            return new RecipeIngredientDto(
                i.Id,
                i.MaterialId,
                m?.Name ?? "(unknown)",
                m?.Category ?? "",
                i.Qty,
                i.Unit,
                i.ComputedCost,
                m?.CostPerBaseUnit ?? 0);
        }).ToList(),
        r.RawMaterialCost,
        r.LaborCost,
        r.OverheadCost,
        r.TotalBatchCost,
        r.CostPerUnit,
        r.CreatedAt,
        r.UpdatedAt
    );
}
