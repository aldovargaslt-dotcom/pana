using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Production;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("recipes")]
public class RecipesController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IRecipeService recipeService,
        CancellationToken ct)
    {
        var recipes = await recipeService.GetAllAsync(ct);
        var vm = recipes.Select(r => new RecipeRowViewModel(
            r.Id, r.Name, r.Yield, r.YieldUnit, r.CostPerUnit, r.TotalBatchCost,
            r.Ingredients.Count, r.IsActive, r.CreatedAt
        )).ToList();

        ViewData["Title"] = "Recetas";
        ViewData["ActiveNav"] = "recipes";

        return View(new RecipeListViewModel(vm, vm.Count));
    }

    [HttpGet("table-rows")]
    public async Task<IActionResult> TableRows(
        [FromServices] IRecipeService recipeService,
        [FromQuery] string? q,
        CancellationToken ct)
    {
        var recipes = await recipeService.GetAllAsync(ct);
        var rows = recipes.Select(r => new RecipeRowViewModel(
            r.Id, r.Name, r.Yield, r.YieldUnit, r.CostPerUnit, r.TotalBatchCost,
            r.Ingredients.Count, r.IsActive, r.CreatedAt
        ));

        if (!string.IsNullOrWhiteSpace(q))
        {
            rows = rows.Where(r =>
                r.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return PartialView("_TableRows", rows.ToList());
    }

    [HttpGet("create-form")]
    public IActionResult CreateForm()
    {
        return PartialView("_Form", new RecipeFormViewModel());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromServices] IRecipeService recipeService,
        [FromForm] RecipeFormViewModel form,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var request = new CreateRecipeRequest(
            form.Name, form.Yield, form.YieldUnit,
            form.LaborCostPerUnit, form.EnergyCost, form.OverheadPct,
            form.ProductId);

        await recipeService.CreateAsync(request, ct);

        Response.Headers["HX-Trigger"] = "recipe-created";
        return await TableRows(recipeService, null, ct);
    }

    [HttpGet("{id:guid}/edit-form")]
    public async Task<IActionResult> EditForm(
        [FromServices] IRecipeService recipeService,
        Guid id,
        CancellationToken ct)
    {
        var recipe = await recipeService.GetByIdAsync(id, ct);
        if (recipe is null) return NotFound();

        var vm = new RecipeFormViewModel(
            recipe.Id, recipe.Name, recipe.Yield, recipe.YieldUnit,
            recipe.LaborCostPerUnit, recipe.EnergyCost, recipe.OverheadPct,
            recipe.ProductId);

        return PartialView("_Form", vm);
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(
        [FromServices] IRecipeService recipeService,
        Guid id,
        [FromForm] RecipeFormViewModel form,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var request = new UpdateRecipeRequest(
            form.Name, form.Yield, form.YieldUnit,
            form.LaborCostPerUnit, form.EnergyCost, form.OverheadPct,
            form.ProductId);

        await recipeService.UpdateAsync(id, request, ct);

        Response.Headers["HX-Trigger"] = "recipe-created";
        return await TableRows(recipeService, null, ct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] IRecipeService recipeService,
        Guid id,
        CancellationToken ct)
    {
        await recipeService.DeleteAsync(id, ct);
        Response.Headers["HX-Trigger"] = "recipe-created";
        return await TableRows(recipeService, null, ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(
        [FromServices] IRecipeService recipeService,
        Guid id,
        CancellationToken ct)
    {
        var recipe = await recipeService.GetByIdAsync(id, ct);
        if (recipe is null) return NotFound();

        ViewData["Title"] = recipe.Name;
        ViewData["ActiveNav"] = "recipes";

        return View(recipe);
    }
}
