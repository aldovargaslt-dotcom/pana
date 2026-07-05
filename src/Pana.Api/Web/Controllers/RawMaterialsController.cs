using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Production;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("raw-materials")]
public class RawMaterialsController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IRawMaterialService materialService,
        CancellationToken ct)
    {
        var materials = await materialService.GetAllAsync(ct: ct);
        var vm = materials.Select(m => new RawMaterialRowViewModel(
            m.Id, m.Name, m.Category, m.PurchaseUnit, m.PurchasePrice,
            m.YieldPct, m.PresentationQty, m.BaseUnit, m.CostPerBaseUnit,
            m.Supplier, m.IsActive, m.CreatedAt
        )).ToList();

        ViewData["Title"] = "Materias Primas";
        ViewData["ActiveNav"] = "raw-materials";

        return View(new RawMaterialListViewModel(vm, vm.Count));
    }

    [HttpGet("table-rows")]
    public async Task<IActionResult> TableRows(
        [FromServices] IRawMaterialService materialService,
        [FromQuery] string? q,
        CancellationToken ct)
    {
        var materials = await materialService.GetAllAsync(ct: ct);
        var rows = materials.Select(m => new RawMaterialRowViewModel(
            m.Id, m.Name, m.Category, m.PurchaseUnit, m.PurchasePrice,
            m.YieldPct, m.PresentationQty, m.BaseUnit, m.CostPerBaseUnit,
            m.Supplier, m.IsActive, m.CreatedAt
        ));

        if (!string.IsNullOrWhiteSpace(q))
        {
            rows = rows.Where(r =>
                r.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (r.Supplier != null && r.Supplier.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        return PartialView("_TableRows", rows.ToList());
    }

    [HttpGet("create-form")]
    public IActionResult CreateForm()
    {
        return PartialView("_Form", new RawMaterialFormViewModel());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromServices] IRawMaterialService materialService,
        [FromForm] RawMaterialFormViewModel form,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var request = new RawMaterialRequest(
            form.Name, form.Category, form.PurchaseUnit, form.PurchasePrice,
            form.PresentationQty, form.BaseUnit, form.YieldPct, form.Supplier);

        await materialService.CreateAsync(request, ct);

        Response.Headers["HX-Trigger"] = "material-created";
        return await TableRows(materialService, null, ct);
    }

    [HttpGet("{id:guid}/edit-form")]
    public async Task<IActionResult> EditForm(
        [FromServices] IRawMaterialService materialService,
        Guid id,
        CancellationToken ct)
    {
        var material = await materialService.GetByIdAsync(id, ct);
        if (material is null) return NotFound();

        var vm = new RawMaterialFormViewModel(
            material.Id, material.Name, material.Category, material.PurchaseUnit,
            material.PurchasePrice, material.PresentationQty, material.BaseUnit,
            material.YieldPct, material.Supplier);

        return PartialView("_Form", vm);
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(
        [FromServices] IRawMaterialService materialService,
        Guid id,
        [FromForm] RawMaterialFormViewModel form,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var request = new RawMaterialRequest(
            form.Name, form.Category, form.PurchaseUnit, form.PurchasePrice,
            form.PresentationQty, form.BaseUnit, form.YieldPct, form.Supplier);

        await materialService.UpdateAsync(id, request, ct);

        Response.Headers["HX-Trigger"] = "material-created";
        return await TableRows(materialService, null, ct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] IRawMaterialService materialService,
        Guid id,
        CancellationToken ct)
    {
        await materialService.DeleteAsync(id, ct);
        Response.Headers["HX-Trigger"] = "material-created";
        return await TableRows(materialService, null, ct);
    }
}
