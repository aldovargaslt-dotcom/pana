namespace Pana.Api.Domain.Production;

/// <summary>
/// Static unit conversion factors for recipe costing.
/// All factors are relative to the base unit (g for weight, mL for volume, pza for count).
/// 
/// Weight:   kg → 1000g,  g → 1g
/// Volume:   L  → 1000mL, mL → 1mL
/// Count:    pza → 1, docena → 12, cono → 1
/// </summary>
public static class UnitConversion
{
    private static readonly Dictionary<string, decimal> Factors = new(StringComparer.OrdinalIgnoreCase)
    {
        // Weight
        ["kg"] = 1000m,
        ["g"]  = 1m,

        // Volume
        ["L"]  = 1000m,
        ["mL"] = 1m,

        // Count
        ["pza"]    = 1m,
        ["docena"] = 12m,
        ["cono"]   = 1m,
    };

    /// <summary>
    /// Returns the conversion factor for a unit relative to its base.
    /// E.g., "kg" → 1000, "g" → 1.
    /// </summary>
    public static decimal GetFactor(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required.", nameof(unit));

        if (Factors.TryGetValue(unit.Trim(), out var factor))
            return factor;

        throw new ArgumentException($"Unknown unit: '{unit}'. Supported units: {string.Join(", ", Factors.Keys)}", nameof(unit));
    }

    /// <summary>
    /// Converts a quantity from one unit to another within the same category.
    /// E.g., 1 kg → 1000 g, 500 g → 0.5 kg.
    /// </summary>
    public static decimal Convert(decimal qty, string fromUnit, string toUnit)
    {
        var fromFactor = GetFactor(fromUnit);
        var toFactor = GetFactor(toUnit);
        return qty * (fromFactor / toFactor);
    }

    /// <summary>
    /// Calculates the cost contribution of an ingredient in a recipe.
    /// Formula: qty × (ingredientUnitFactor / materialBaseUnitFactor) × materialCostPerBaseUnit
    /// </summary>
    public static decimal CalculateIngredientCost(
        decimal qty,
        string ingredientUnit,
        string materialBaseUnit,
        decimal materialCostPerBaseUnit)
    {
        var ingredientFactor = GetFactor(ingredientUnit);
        var materialFactor = GetFactor(materialBaseUnit);

        // Convert ingredient quantity to material's base unit, then multiply by cost
        var qtyInMaterialBaseUnit = qty * (ingredientFactor / materialFactor);
        return qtyInMaterialBaseUnit * materialCostPerBaseUnit;
    }

    /// <summary>
    /// Returns all supported units.
    /// </summary>
    public static IReadOnlyCollection<string> SupportedUnits => Factors.Keys.ToList().AsReadOnly();
}
