namespace Pana.Api.Application.Sales;

// ── DTOs ────────────────────────────────────────────────────────

public record SaleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    bool IsVoided
);

public record SaleDto(
    Guid Id,
    string Status,
    decimal TotalAmount,
    string? Notes,
    Guid? SoldByUserId,
    DateTime CreatedAt,
    List<SaleItemDto> Items
);

public record CreateSaleItemRequest(
    Guid ProductId,
    decimal UnitPrice,
    int Quantity
);

public record CreateSaleRequest(
    List<CreateSaleItemRequest> Items,
    string? Notes = null
);

public record DailySalesSummary(
    DateTime Date,
    int SaleCount,
    decimal TotalRevenue,
    decimal TotalMargin
);
