using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Production;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/recipes")]
[Produces("application/json")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    /// <summary>
    /// Get all recipes for the current tenant with full cost breakdown.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RecipeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var recipes = await _recipeService.GetAllAsync(ct);
        return Ok(recipes);
    }

    /// <summary>
    /// Get a recipe by ID with full cost breakdown.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var recipe = await _recipeService.GetByIdAsync(id, ct);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    /// <summary>
    /// Create a new recipe with optional ingredients.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request, CancellationToken ct)
    {
        var recipe = await _recipeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }

    /// <summary>
    /// Update an existing recipe (partial update).
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeRequest request, CancellationToken ct)
    {
        var recipe = await _recipeService.UpdateAsync(id, request, ct);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    /// <summary>
    /// Delete (deactivate) a recipe.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _recipeService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Set (replace) all ingredients for a recipe. Recalculates costs automatically.
    /// </summary>
    [HttpPut("{id:guid}/ingredients")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetIngredients(Guid id, [FromBody] SetRecipeIngredientsRequest request, CancellationToken ct)
    {
        var recipe = await _recipeService.SetIngredientsAsync(id, request, ct);
        return recipe is null ? NotFound() : Ok(recipe);
    }

    /// <summary>
    /// Recalculate all ingredient costs based on current material prices.
    /// </summary>
    [HttpPost("{id:guid}/recalculate")]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecalculateCosts(Guid id, CancellationToken ct)
    {
        var recipe = await _recipeService.RecalculateCostsAsync(id, ct);
        return recipe is null ? NotFound() : Ok(recipe);
    }
}
