namespace Pana.Api.Application.Products;

/// <summary>
/// Data transfer object for product responses.
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    decimal Cost,
    decimal Margin,
    decimal MarginPercentage,
    bool IsActive,
    string ProductType,
    Guid? UnitOfMeasureId,
    Guid? PurchaseUnitOfMeasureId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// UoM Data Transfer Object.
/// </summary>
public record UnitOfMeasureDto(
    Guid Id,
    string Name,
    string Symbol,
    string Category,
    decimal Factor,
    decimal RoundingPrecision,
    bool IsActive
);

/// <summary>
/// Request to create or update a product.
/// </summary>
public record ProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Cost { get; init; }
    public string? Description { get; init; }
    public string? ProductType { get; init; }
    public Guid? UnitOfMeasureId { get; init; }
    public Guid? PurchaseUnitOfMeasureId { get; init; }
}
