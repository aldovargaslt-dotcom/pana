namespace Pana.Api.Application.Inventory;

/// <summary>
/// DTOs for WasteCategory — categorizes production waste for root-cause analysis.
/// </summary>
public class WasteCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public List<WasteSubcategoryDto> Subcategories { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public class WasteSubcategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class WasteCategoryCreateRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public List<string>? Subcategories { get; init; }
}

public class WasteCategoryUpdateRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public int? SortOrder { get; init; }
}
