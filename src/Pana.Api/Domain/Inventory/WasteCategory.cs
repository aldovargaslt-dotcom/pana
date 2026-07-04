namespace Pana.Api.Domain.Inventory;

using Pana.Api.Domain.Common;

/// <summary>
/// Categorizes waste/return reasons for inventory movements of type Waste or Return.
/// Enables root-cause analysis of production losses.
/// Extracted from real bakery operations knowledge.
/// </summary>
public class WasteCategory : TenantEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<WasteSubcategory> _subcategories = new();
    public IReadOnlyCollection<WasteSubcategory> Subcategories => _subcategories.AsReadOnly();

    private WasteCategory() { } // EF Core

    public WasteCategory(Guid tenantId, string name, int sortOrder = 0, string? description = null)
        : base(tenantId)
    {
        SetName(name);
        SortOrder = sortOrder;
        Description = description?.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Waste category name is required.", nameof(name));
        Name = name.Trim();
        MarkUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        MarkUpdated();
    }

    public void SetSortOrder(int order)
    {
        SortOrder = order;
        MarkUpdated();
    }

    public void Activate() { IsActive = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }

    public WasteSubcategory AddSubcategory(string name)
    {
        var sub = new WasteSubcategory(Id, name);
        _subcategories.Add(sub);
        return sub;
    }
}
