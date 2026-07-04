namespace Pana.Api.Domain.Products;

using Pana.Api.Domain.Common;

/// <summary>
/// Unit of Measure with category-based conversion support.
/// Inspired by Odoo's uom.uom — enables selling in units while buying in bulk.
/// Example: Buy flour in kg (factor=1), sell bread in units (factor=1, different category).
/// </summary>
public class UnitOfMeasure : TenantEntity
{
    public string Name { get; private set; }
    public string Symbol { get; private set; }
    public string Category { get; private set; }
    public decimal Factor { get; private set; }
    /// <summary>
    /// Rounding precision for this UoM (e.g., 0.01 for kg, 1 for units).
    /// </summary>
    public decimal RoundingPrecision { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static class Categories
    {
        public const string Weight = "Weight";
        public const string Volume = "Volume";
        public const string Unit = "Unit";
        public const string Length = "Length";
        public const string Time = "Time";
    }

    private UnitOfMeasure() { }

    public UnitOfMeasure(Guid tenantId, string name, string symbol, string category, decimal factor, decimal roundingPrecision = 1)
        : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("UoM name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("UoM symbol is required.", nameof(symbol));
        if (factor <= 0)
            throw new ArgumentException("Factor must be positive.", nameof(factor));

        Name = name.Trim();
        Symbol = symbol.Trim();
        Category = category;
        Factor = factor;
        RoundingPrecision = roundingPrecision;
    }

    /// <summary>
    /// Convert a quantity from this UoM to the reference UoM of the same category.
    /// Reference UoM always has factor = 1.
    /// </summary>
    public decimal ToReference(decimal quantity) => quantity * Factor;

    /// <summary>
    /// Convert a quantity from the reference UoM to this UoM.
    /// </summary>
    public decimal FromReference(decimal quantity) => quantity / Factor;

    /// <summary>
    /// Convert a quantity from this UoM to another UoM in the same category.
    /// </summary>
    public decimal ConvertTo(decimal quantity, UnitOfMeasure target)
    {
        if (Category != target.Category)
            throw new InvalidOperationException($"Cannot convert from {Category} to {target.Category}. Categories must match.");

        var inReference = ToReference(quantity);
        return target.FromReference(inReference);
    }
}
