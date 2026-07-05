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
    List<SaleItemDto> Items,
    // Pre-order / customer fields
    string OrderType,
    string? CustomerName,
    string? CustomerPhone,
    DateTime? ScheduledDate,
    decimal DepositAmount,
    decimal BalanceDue,
    string PaymentStatus,
    string? PaymentMethod,
    string? InternalNotes
);

public record CreateSaleItemRequest(
    Guid ProductId,
    decimal UnitPrice,
    int Quantity
);

public record CreateSaleRequest(
    List<CreateSaleItemRequest> Items,
    string? Notes = null,
    string OrderType = "Standard",
    string? CustomerName = null,
    string? CustomerPhone = null,
    DateTime? ScheduledDate = null,
    decimal DepositAmount = 0,
    string? PaymentMethod = null,
    string? InternalNotes = null
);

public record UpdatePreOrderRequest(
    string? CustomerName = null,
    string? CustomerPhone = null,
    DateTime? ScheduledDate = null,
    decimal? DepositAmount = null,
    string? InternalNotes = null
);

public record RecordPaymentRequest(
    decimal Amount
);

public record DailySalesSummary(
    DateTime Date,
    int SaleCount,
    decimal TotalRevenue,
    decimal TotalMargin
);
