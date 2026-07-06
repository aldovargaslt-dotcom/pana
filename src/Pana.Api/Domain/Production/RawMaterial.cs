namespace Pana.Api.Domain.Production;

using Pana.Api.Domain.Common;

/// <summary>
/// A raw material or ingredient used in production recipes.
/// Tracks purchase cost, yield percentage (processing loss), and supplier info.
/// The effective cost per base unit is automatically calculated.
/// Extracted from real bakery operations knowledge.
/// </summary>
public class RawMaterial : TenantEntity
{
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string PurchaseUnit { get; private set; }
    public decimal PurchasePrice { get; private set; }
    /// <summary>
    /// Percentage of the material that is actually usable after processing (1-100).
    /// Example: flour at 95% means 5% is lost to dusting/spillage.
    /// </summary>
    public decimal YieldPct { get; private set; }
    /// <summary>
    /// Quantity of the purchase unit (e.g., 500 for a 500g bag).
    /// </summary>
    public decimal PresentationQty { get; private set; }
    /// <summary>
    /// Base unit for cost calculation (g, mL, pza).
    /// </summary>
    public string BaseUnit { get; private set; }
    public string? Supplier { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Computed effective cost per base unit, accounting for yield loss
    /// and converting from purchase unit to base unit.
    /// Formula: purchasePrice / (presentationQty × purchaseToBaseFactor × yieldPct/100)
    /// </summary>
    public decimal CostPerBaseUnit
    {
        get
        {
            if (PresentationQty <= 0 || YieldPct <= 0) return 0;
            var factor = UnitConversion.GetFactor(PurchaseUnit);
            var baseFactor = UnitConversion.GetFactor(BaseUnit);
            // Convert presentation qty from purchase unit to base unit
            var qtyInBaseUnit = PresentationQty * (factor / baseFactor);
            return PurchasePrice / (qtyInBaseUnit * (YieldPct / 100m));
        }
    }

    public static class Categories
    {
        public const string Flours = "Flours";
        public const string Sugars = "Sugars";
        public const string Eggs = "Eggs";
        public const string Dairy = "Dairy";
        public const string Fats = "Fats";
        public const string Leaveners = "Leaveners";
        public const string Flavorings = "Flavorings";
        public const string Other = "Other";
    }

    public static class BaseUnits
    {
        public const string Gram = "g";
        public const string Milliliter = "mL";
        public const string Piece = "pza";
    }

    public static class PurchaseUnits
    {
        public const string Kg = "kg";
        public const string Gram = "g";
        public const string Liter = "L";
        public const string Milliliter = "mL";
        public const string Piece = "pza";
        public const string Dozen = "docena";
        public const string Cone = "cono";
    }

    private RawMaterial() { } // EF Core

    public RawMaterial(
        Guid tenantId,
        string name,
        string category,
        string purchaseUnit,
        decimal purchasePrice,
        decimal presentationQty,
        string baseUnit,
        decimal yieldPct = 100,
        string? supplier = null)
        : base(tenantId)
    {
        SetName(name);
        SetCategory(category);
        SetPurchaseInfo(purchaseUnit, purchasePrice, presentationQty, baseUnit);
        SetYield(yieldPct);
        Supplier = supplier?.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Material name is required.", nameof(name));
        Name = name.Trim();
        MarkUpdated();
    }

    public void SetCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required.", nameof(category));
        Category = category.Trim();
        MarkUpdated();
    }

    public void SetPurchaseInfo(string purchaseUnit, decimal purchasePrice, decimal presentationQty, string baseUnit)
    {
        if (string.IsNullOrWhiteSpace(purchaseUnit))
            throw new ArgumentException("Purchase unit is required.", nameof(purchaseUnit));
        if (purchasePrice < 0)
            throw new ArgumentException("Purchase price cannot be negative.", nameof(purchasePrice));
        if (presentationQty <= 0)
            throw new ArgumentException("Presentation quantity must be positive.", nameof(presentationQty));
        if (string.IsNullOrWhiteSpace(baseUnit))
            throw new ArgumentException("Base unit is required.", nameof(baseUnit));

        PurchaseUnit = purchaseUnit.Trim();
        PurchasePrice = purchasePrice;
        PresentationQty = presentationQty;
        BaseUnit = baseUnit.Trim();
        MarkUpdated();
    }

    public void SetYield(decimal yieldPct)
    {
        if (yieldPct <= 0 || yieldPct > 100)
            throw new ArgumentException("Yield percentage must be between 0 and 100.", nameof(yieldPct));
        YieldPct = yieldPct;
        MarkUpdated();
    }

    public void SetSupplier(string? supplier)
    {
        Supplier = supplier?.Trim();
        MarkUpdated();
    }

    public void Activate() { IsActive = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }
}
