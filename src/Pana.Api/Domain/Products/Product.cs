namespace Pana.Api.Domain.Products;

using Pana.Api.Domain.Common;

/// <summary>
/// Represents a product or service sold by a business.
/// </summary>
public class Product : TenantEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Sku { get; private set; }
    public decimal Price { get; private set; }
    public decimal Cost { get; private set; }
    public bool IsActive { get; private set; } = true;

    // ── UoM support (Odoo-inspired) ──────────────────────────
    public Guid? UnitOfMeasureId { get; private set; }
    public Guid? PurchaseUnitOfMeasureId { get; private set; }

    // Product type: storable (track inventory) vs service/consumable
    public string ProductType { get; private set; } = "Storable";

    public static class ProductTypes
    {
        public const string Storable = "Storable";
        public const string Consumable = "Consumable";
        public const string Service = "Service";
    }

    private Product() { } // EF Core

    public Product(Guid tenantId, string name, string sku, decimal price, decimal cost)
        : base(tenantId)
    {
        SetName(name);
        SetSku(sku);
        SetPricing(price, cost);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        Name = name.Trim();
        MarkUpdated();
    }

    public void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        Sku = sku.Trim().ToUpperInvariant();
        MarkUpdated();
    }

    public void SetPricing(decimal price, decimal cost)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (cost < 0)
            throw new ArgumentException("Cost cannot be negative.", nameof(cost));
        Price = price;
        Cost = cost;
        MarkUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void SetUnitOfMeasure(Guid? uomId)
    {
        UnitOfMeasureId = uomId;
        MarkUpdated();
    }

    public void SetPurchaseUnitOfMeasure(Guid? purchaseUomId)
    {
        PurchaseUnitOfMeasureId = purchaseUomId;
        MarkUpdated();
    }

    public void SetProductType(string productType)
    {
        if (productType is not (ProductTypes.Storable or ProductTypes.Consumable or ProductTypes.Service))
            throw new ArgumentException($"Invalid product type: {productType}.", nameof(productType));
        ProductType = productType;
        MarkUpdated();
    }

    public decimal Margin => Price - Cost;
    public decimal MarginPercentage => Price > 0 ? (Margin / Price) * 100 : 0;
}
