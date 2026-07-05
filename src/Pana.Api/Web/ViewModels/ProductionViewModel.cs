namespace Pana.Api.Web.ViewModels;

// ── Raw Materials ──────────────────────────────────────────────

public record RawMaterialListViewModel(
    List<RawMaterialRowViewModel> Materials,
    int TotalCount
);

public record RawMaterialRowViewModel(
    Guid Id,
    string Name,
    string Category,
    string PurchaseUnit,
    decimal PurchasePrice,
    decimal YieldPct,
    decimal PresentationQty,
    string BaseUnit,
    decimal CostPerBaseUnit,
    string? Supplier,
    bool IsActive,
    DateTime CreatedAt
);

public record RawMaterialFormViewModel(
    Guid? Id = null,
    string Name = "",
    string Category = "",
    string PurchaseUnit = "",
    decimal PurchasePrice = 0,
    decimal PresentationQty = 1,
    string BaseUnit = "g",
    decimal YieldPct = 100,
    string? Supplier = null
);

// ── Recipes ────────────────────────────────────────────────────

public record RecipeListViewModel(
    List<RecipeRowViewModel> Recipes,
    int TotalCount
);

public record RecipeRowViewModel(
    Guid Id,
    string Name,
    decimal Yield,
    string YieldUnit,
    decimal CostPerUnit,
    decimal TotalBatchCost,
    bool IsActive,
    DateTime CreatedAt
);

public record RecipeFormViewModel(
    Guid? Id = null,
    string Name = "",
    decimal Yield = 1,
    string YieldUnit = "pza",
    decimal LaborCostPerUnit = 0,
    decimal EnergyCost = 0,
    decimal OverheadPct = 0,
    Guid? ProductId = null
);
