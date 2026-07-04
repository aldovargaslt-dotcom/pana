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
