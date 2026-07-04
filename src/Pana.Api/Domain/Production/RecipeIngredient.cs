namespace Pana.Api.Domain.Production;

using Pana.Api.Domain.Common;

/// <summary>
/// An ingredient line within a recipe.
/// Links a raw material with a quantity and unit used in production.
/// The computed cost is calculated from the material's CostPerBaseUnit
/// multiplied by a unit conversion factor.
/// </summary>
public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public Guid MaterialId { get; private set; }
    /// <summary>Quantity of this ingredient used in the recipe.</summary>
    public decimal Qty { get; private set; }
    /// <summary>Unit of measurement for this ingredient (may differ from material's purchase unit).</summary>
    public string Unit { get; private set; }
    /// <summary>
    /// Pre-computed cost contribution of this ingredient.
    /// Formula: qty × conversionFactor × material.CostPerBaseUnit
    /// Updated when material prices change.
    /// </summary>
    public decimal ComputedCost { get; private set; }

    private RecipeIngredient() { } // EF Core

    public RecipeIngredient(Guid recipeId, Guid materialId, decimal qty, string unit, decimal computedCost = 0)
    {
        RecipeId = recipeId;
        MaterialId = materialId;
        SetQuantityAndUnit(qty, unit);
        ComputedCost = computedCost;
    }

    public void SetQuantityAndUnit(decimal qty, string unit)
    {
        if (qty <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(qty));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required.", nameof(unit));
        Qty = qty;
        Unit = unit.Trim();
        MarkUpdated();
    }

    public void SetComputedCost(decimal materialCostPerBaseUnit)
    {
        var factor = UnitConversion.GetFactor(Unit);
        ComputedCost = Qty * factor * materialCostPerBaseUnit;
        MarkUpdated();
    }
}
