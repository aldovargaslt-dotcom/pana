namespace Pana.Api.Domain.Inventory;

using Pana.Api.Domain.Common;

/// <summary>
/// Hierarchical inventory location (Warehouse → Zone → Shelf → Bin).
/// Inspired by Odoo's stock.location — enables physical organization of stock.
/// </summary>
public class StockLocation : TenantEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string LocationType { get; private set; }
    public Guid? ParentLocationId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static class Types
    {
        public const string Warehouse = "Warehouse";
        public const string Zone = "Zone";
        public const string Shelf = "Shelf";
        public const string Bin = "Bin";
        public const string Supplier = "Supplier";
        public const string Customer = "Customer";
        public const string Production = "Production";
        public const string Virtual = "Virtual";
    }

    private StockLocation() { }

    public StockLocation(Guid tenantId, string name, string code, string locationType, Guid? parentLocationId = null)
        : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Location code is required.", nameof(code));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        LocationType = locationType;
        ParentLocationId = parentLocationId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name is required.", nameof(name));
        Name = name.Trim();
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

    public void MoveTo(Guid? parentLocationId)
    {
        ParentLocationId = parentLocationId;
        MarkUpdated();
    }
}
