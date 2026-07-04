namespace Pana.Api.Application.Inventory;

// ── Stock Location DTOs ─────────────────────────────────────────

public record StockLocationDto(
    Guid Id,
    string Name,
    string Code,
    string LocationType,
    Guid? ParentLocationId,
    bool IsActive
);

public record CreateStockLocationRequest(
    string Name,
    string Code,
    string LocationType,
    Guid? ParentLocationId = null
);

// ── Reorder Rule DTOs ───────────────────────────────────────────

public record ReorderRuleDto(
    Guid Id,
    Guid ProductId,
    Guid LocationId,
    decimal MinimumStock,
    decimal MaximumStock,
    decimal ReorderQuantity,
    bool IsActive,
    DateTime? LastTriggeredAt
);

public record CreateReorderRuleRequest(
    Guid ProductId,
    Guid LocationId,
    decimal MinimumStock,
    decimal MaximumStock,
    decimal ReorderQuantity
);

public record ReorderSuggestionDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal CurrentStock,
    decimal MinimumStock,
    decimal SuggestedOrderQuantity
);
