namespace Pana.Api.Application.Production;

/// <summary>
/// DTO for RawMaterial responses.
/// CostPerBaseUnit is calculated on read, not stored.
/// </summary>
public record RawMaterialDto(
    Guid Id,
    string Name,
    string Category,
    string PurchaseUnit,
    decimal PurchasePrice,
    decimal YieldPct,
    decimal PresentationQty,
    string BaseUnit,
    string? Supplier,
    bool IsActive,
    decimal CostPerBaseUnit,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request to create or update a raw material.
/// </summary>
public record RawMaterialRequest(
    string Name,
    string Category,
    string PurchaseUnit,
    decimal PurchasePrice,
    decimal PresentationQty,
    string BaseUnit,
    decimal YieldPct = 100,
    string? Supplier = null
);
