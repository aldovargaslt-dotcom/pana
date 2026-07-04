namespace Pana.Api.Domain.Inventory;

/// <summary>
/// Subcategory within a waste category for granular tracking.
/// Example: Category "Quemado" → Subcategories "Horno", "Descuido".
/// </summary>
public class WasteSubcategory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WasteCategoryId { get; private set; }
    public string Name { get; private set; }

    private WasteSubcategory() { } // EF Core

    public WasteSubcategory(Guid wasteCategoryId, string name)
    {
        WasteCategoryId = wasteCategoryId;
        SetName(name);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subcategory name is required.", nameof(name));
        Name = name.Trim();
    }
}
