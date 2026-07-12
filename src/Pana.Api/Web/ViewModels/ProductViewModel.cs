namespace Pana.Api.Web.ViewModels;

public record ProductListViewModel(
    List<ProductRowViewModel> Products,
    int TotalCount
);

public record ProductRowViewModel(
    Guid Id,
    string Name,
    string Sku,
    decimal Price,
    decimal Cost,
    decimal Margin,
    decimal MarginPercentage,
    bool IsActive,
    DateTime CreatedAt,
    string? ImageUrl = null
);

public record ProductFormViewModel(
    Guid? Id = null,
    string Name = "",
    string Sku = "",
    decimal Price = 0,
    decimal Cost = 0,
    string? Description = null,
    string? ImageUrl = null
);
