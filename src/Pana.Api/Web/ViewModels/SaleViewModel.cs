namespace Pana.Api.Web.ViewModels;

public record SaleListViewModel(
    List<SaleRowViewModel> Sales,
    int TotalCount
);

public record SaleRowViewModel(
    Guid Id,
    string Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    string OrderType = "Standard",
    string? CustomerName = null,
    string PaymentStatus = "Unpaid",
    DateTime? ScheduledDate = null
);

public record SaleFormViewModel(
    List<SaleItemFormViewModel>? Items = null,
    string? Notes = null,
    string OrderType = "Standard",
    string? CustomerName = null,
    string? CustomerPhone = null,
    DateTime? ScheduledDate = null,
    decimal DepositAmount = 0,
    string? InternalNotes = null
);

public record SaleItemFormViewModel(
    Guid ProductId = default,
    decimal UnitPrice = 0,
    int Quantity = 1
);
