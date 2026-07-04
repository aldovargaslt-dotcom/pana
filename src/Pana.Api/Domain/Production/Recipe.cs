namespace Pana.Api.Domain.Production;

using Pana.Api.Domain.Common;

/// <summary>
/// A production recipe that defines how raw materials combine to create a product.
/// Includes cost modifiers: labor, energy, and overhead percentage.
/// Recipes can be scaled (0.5x, 1x, 2x, 3x) for different batch sizes.
/// Optionally linked to a sellable Product for automatic COGS calculation.
/// </summary>
public class Recipe : TenantEntity
{
    public string Name { get; private set; }
    /// <summary>
    /// How many units this recipe produces.
    /// </summary>
    public decimal Yield { get; private set; }
    /// <summary>
    /// Unit of the yield (piezas, kg, docenas, etc.).
    /// </summary>
    public string YieldUnit { get; private set; }

    // ── Cost modifiers ───────────────────────────────────────
    /// <summary>Labor cost per yielded unit (e.g., $0.50 per piece).</summary>
    public decimal LaborCostPerUnit { get; private set; }
    /// <summary>Fixed energy cost per batch (gas + electricity).</summary>
    public decimal EnergyCost { get; private set; }
    /// <summary>Overhead percentage applied on top of raw material cost (e.g., 5 = 5%).</summary>
    public decimal OverheadPct { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>Optional link to a sellable Product for automatic COGS.</summary>
    public Guid? ProductId { get; private set; }

    private readonly List<RecipeIngredient> _ingredients = new();
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    // ── Computed costs ───────────────────────────────────────

    /// <summary>Sum of all ingredient costs (requires material CostPerBaseUnit).</summary>
    public decimal RawMaterialCost => _ingredients.Sum(i => i.ComputedCost);

    /// <summary>laborCostPerUnit × yield</summary>
    public decimal LaborCost => LaborCostPerUnit * Yield;

    /// <summary>rawMaterialCost × (overheadPct / 100)</summary>
    public decimal OverheadCost => RawMaterialCost * (OverheadPct / 100m);

    /// <summary>Raw materials + labor + energy + overhead</summary>
    public decimal TotalBatchCost => RawMaterialCost + LaborCost + EnergyCost + OverheadCost;

    /// <summary>Cost per single yielded unit</summary>
    public decimal CostPerUnit => Yield > 0 ? TotalBatchCost / Yield : 0;

    private Recipe() { } // EF Core

    public Recipe(
        Guid tenantId,
        string name,
        decimal yield,
        string yieldUnit,
        decimal laborCostPerUnit = 0,
        decimal energyCost = 0,
        decimal overheadPct = 0,
        Guid? productId = null)
        : base(tenantId)
    {
        SetName(name);
        SetYield(yield, yieldUnit);
        SetLaborCost(laborCostPerUnit);
        SetEnergyCost(energyCost);
        SetOverhead(overheadPct);
        ProductId = productId;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name is required.", nameof(name));
        Name = name.Trim();
        MarkUpdated();
    }

    public void SetYield(decimal yield, string yieldUnit)
    {
        if (yield <= 0)
            throw new ArgumentException("Yield must be positive.", nameof(yield));
        if (string.IsNullOrWhiteSpace(yieldUnit))
            throw new ArgumentException("Yield unit is required.", nameof(yieldUnit));
        Yield = yield;
        YieldUnit = yieldUnit.Trim();
        MarkUpdated();
    }

    public void SetLaborCost(decimal costPerUnit)
    {
        if (costPerUnit < 0)
            throw new ArgumentException("Labor cost cannot be negative.", nameof(costPerUnit));
        LaborCostPerUnit = costPerUnit;
        MarkUpdated();
    }

    public void SetEnergyCost(decimal cost)
    {
        if (cost < 0)
            throw new ArgumentException("Energy cost cannot be negative.", nameof(cost));
        EnergyCost = cost;
        MarkUpdated();
    }

    public void SetOverhead(decimal overheadPct)
    {
        if (overheadPct < 0 || overheadPct > 100)
            throw new ArgumentException("Overhead percentage must be between 0 and 100.", nameof(overheadPct));
        OverheadPct = overheadPct;
        MarkUpdated();
    }

    public void SetProduct(Guid? productId)
    {
        ProductId = productId;
        MarkUpdated();
    }

    public void Activate() { IsActive = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }

    public RecipeIngredient AddIngredient(Guid materialId, decimal qty, string unit, decimal computedCost = 0)
    {
        if (qty <= 0)
            throw new ArgumentException("Ingredient quantity must be positive.", nameof(qty));

        var ingredient = new RecipeIngredient(Id, materialId, qty, unit, computedCost);
        _ingredients.Add(ingredient);
        MarkUpdated();
        return ingredient;
    }

    public void RemoveIngredient(Guid ingredientId)
    {
        var ingredient = _ingredients.FirstOrDefault(i => i.Id == ingredientId);
        if (ingredient is not null)
        {
            _ingredients.Remove(ingredient);
            MarkUpdated();
        }
    }

    public void ClearIngredients()
    {
        _ingredients.Clear();
        MarkUpdated();
    }

    /// <summary>
    /// Updates all ingredient computed costs based on current material prices.
    /// Call this after material prices change to keep recipe costs up to date.
    /// </summary>
    public void RecalculateIngredientCosts(Func<Guid, decimal> getMaterialCostPerBaseUnit)
    {
        foreach (var ingredient in _ingredients)
        {
            var materialCost = getMaterialCostPerBaseUnit(ingredient.MaterialId);
            ingredient.SetComputedCost(materialCost);
        }
        MarkUpdated();
    }
}
