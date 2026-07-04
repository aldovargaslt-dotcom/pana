namespace Pana.Api.Application.Inventory;

// ── DTOs ────────────────────────────────────────────────────────

public record InventoryMovementDto(
    Guid Id,
    Guid ProductId,
    string MovementType,
    decimal Quantity,
    string? Reason,
    Guid? ReferenceSaleId,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    DateTime CreatedAt
);

public record StockLevelDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal CurrentStock,
    decimal AverageCost
);

public record StockInRequest(
    Guid ProductId,
    decimal Quantity,
    string? Reason = null
);

public record StockOutRequest(
    Guid ProductId,
    decimal Quantity,
    string? Reason = null
);

public record AdjustmentRequest(
    Guid ProductId,
    decimal NewStockLevel,
    string? Reason = null
);
