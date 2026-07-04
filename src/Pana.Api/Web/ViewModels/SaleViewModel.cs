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
    DateTime CreatedAt
);

public record SaleFormViewModel(
    List<SaleItemFormViewModel>? Items = null,
    string? Notes = null
);

public record SaleItemFormViewModel(
    Guid ProductId = default,
    decimal UnitPrice = 0,
    int Quantity = 1
);
