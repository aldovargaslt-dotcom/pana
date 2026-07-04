namespace Pana.Api.Application.Production;

/// <summary>
/// DTO for Recipe responses with full cost breakdown.
/// </summary>
public record RecipeDto(
    Guid Id,
    string Name,
    decimal Yield,
    string YieldUnit,
    decimal LaborCostPerUnit,
    decimal EnergyCost,
    decimal OverheadPct,
    bool IsActive,
    Guid? ProductId,
    List<RecipeIngredientDto> Ingredients,
    // ── Computed costs ──
    decimal RawMaterialCost,
    decimal LaborCost,
    decimal OverheadCost,
    decimal TotalBatchCost,
    decimal CostPerUnit,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO for an ingredient within a recipe.
/// </summary>
public record RecipeIngredientDto(
    Guid Id,
    Guid MaterialId,
    string MaterialName,
    string MaterialCategory,
    decimal Qty,
    string Unit,
    decimal ComputedCost,
    decimal MaterialCostPerBaseUnit
);

/// <summary>
/// Request to create a new recipe.
/// </summary>
public record CreateRecipeRequest(
    string Name,
    decimal Yield,
    string YieldUnit,
    decimal LaborCostPerUnit = 0,
    decimal EnergyCost = 0,
    decimal OverheadPct = 0,
    Guid? ProductId = null,
    List<CreateRecipeIngredientRequest>? Ingredients = null
);

/// <summary>
/// Request to update an existing recipe.
/// </summary>
public record UpdateRecipeRequest(
    string? Name = null,
    decimal? Yield = null,
    string? YieldUnit = null,
    decimal? LaborCostPerUnit = null,
    decimal? EnergyCost = null,
    decimal? OverheadPct = null,
    Guid? ProductId = null
);

/// <summary>
/// Request to add/update an ingredient in a recipe.
/// </summary>
public record CreateRecipeIngredientRequest(
    Guid MaterialId,
    decimal Qty,
    string Unit
);

/// <summary>
/// Request to set all ingredients for a recipe (replaces existing).
/// </summary>
public record SetRecipeIngredientsRequest(
    List<CreateRecipeIngredientRequest> Ingredients
);
