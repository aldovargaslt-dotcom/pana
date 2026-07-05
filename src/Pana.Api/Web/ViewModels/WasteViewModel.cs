using Pana.Api.Application.Inventory;

namespace Pana.Api.Web.ViewModels;

public record WasteIndexViewModel(
    List<WasteCategoryDto> Categories,
    List<WasteRecordViewModel> RecentRecords
);

public record WasteRecordViewModel(
    Guid Id,
    string ProductName,
    decimal Quantity,
    string? CategoryName,
    string? Reason,
    DateTime CreatedAt
);

public record WasteRecordFormViewModel(
    Guid? ProductId = null,
    decimal Quantity = 1,
    Guid? WasteCategoryId = null,
    Guid? WasteSubcategoryId = null,
    string? Reason = null
);
