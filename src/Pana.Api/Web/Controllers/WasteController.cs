using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pana.Api.Application.Inventory;
using Pana.Api.Application.Products;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Route("waste")]
public class WasteController : Controller
{
    private async Task<List<WasteRecordViewModel>> GetRecentRecords(PanaDbContext db, int take = 20, CancellationToken ct = default)
    {
        var movements = await db.InventoryMovements
            .Where(m => m.WasteCategoryId != null)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

        var productIds = movements.Select(m => m.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var categoryIds = movements.Where(m => m.WasteCategoryId.HasValue)
            .Select(m => m.WasteCategoryId!.Value).Distinct().ToList();
        var categories = await db.WasteCategories
            .Where(w => categoryIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, ct);

        return movements.Select(m => new WasteRecordViewModel(
            m.Id,
            products.TryGetValue(m.ProductId, out var p) ? p.Name : "—",
            m.Quantity,
            m.WasteCategoryId.HasValue && categories.TryGetValue(m.WasteCategoryId.Value, out var cat) ? cat.Name : "—",
            m.Reason,
            m.CreatedAt
        )).ToList();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IWasteCategoryService wasteService,
        [FromServices] PanaDbContext db,
        CancellationToken ct)
    {
        var categories = await wasteService.GetAllAsync(ct);
        var recentRecords = await GetRecentRecords(db, 20, ct);

        ViewData["Title"] = "Desperdicios";

        return View(new WasteIndexViewModel(categories, recentRecords));
    }

    [HttpGet("record-form")]
    public async Task<IActionResult> RecordForm(
        [FromServices] IWasteCategoryService wasteService,
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        ViewBag.Products = (await productService.GetAllAsync(ct))
            .Where(p => p.IsActive)
            .Select(p =>
            {
                dynamic obj = new System.Dynamic.ExpandoObject();
                obj.Id = p.Id;
                obj.Name = p.Name;
                return obj;
            })
            .ToList();

        ViewBag.Categories = await wasteService.GetAllAsync(ct);

        return PartialView("_Form", new WasteRecordFormViewModel());
    }

    [HttpPost("record")]
    public async Task<IActionResult> Record(
        [FromServices] PanaDbContext db,
        [FromServices] IWasteCategoryService wasteService,
        [FromForm] WasteRecordFormViewModel form,
        CancellationToken ct)
    {
        if (form.ProductId == null || form.Quantity <= 0)
        {
            if (form.ProductId == null)
                ModelState.AddModelError("ProductId", "Seleccioná un producto.");
            if (form.Quantity <= 0)
                ModelState.AddModelError("Quantity", "La cantidad debe ser mayor a 0.");
        }

        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            var productService = HttpContext.RequestServices.GetRequiredService<IProductService>();
            ViewBag.Products = (await productService.GetAllAsync(ct))
                .Where(p => p.IsActive)
                .Select(p =>
                {
                    dynamic obj = new System.Dynamic.ExpandoObject();
                    obj.Id = p.Id;
                    obj.Name = p.Name;
                    return obj;
                })
                .ToList();
            ViewBag.Categories = await wasteService.GetAllAsync(ct);
            return PartialView("_Form", form);
        }

        var movement = new InventoryMovement(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            form.ProductId!.Value,
            "StockOut",
            -Math.Abs(form.Quantity),
            form.Reason ?? "Desperdicio registrado",
            performedByUserId: Guid.Empty,
            wasteCategoryId: form.WasteCategoryId,
            wasteSubcategoryId: form.WasteSubcategoryId);

        db.InventoryMovements.Add(movement);
        await db.SaveChangesAsync(ct);

        Response.Headers["HX-Trigger"] = "waste-recorded";
        return await TableRows(db, ct);
    }

    [HttpGet("table-rows")]
    public async Task<IActionResult> TableRows(
        [FromServices] PanaDbContext db,
        CancellationToken ct)
    {
        var records = await GetRecentRecords(db, 20, ct);
        return PartialView("_TableRows", records);
    }
}
