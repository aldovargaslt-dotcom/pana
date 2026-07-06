using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Operations;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;

namespace Pana.Api.Web.Controllers;

/// <summary>
/// Daily inventory reconciliation — unified Production + Waste + Sales view.
/// Formula: yesterday's leftover + today's production − waste/returns − sales = tomorrow's leftover.
/// </summary>
[Authorize]
[Route("inventory")]
public class InventoryController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IProductionCaptureService captureService,
        [FromServices] IProductService productService,
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var today = await captureService.GetTodayAsync(ct);
        var products = await productService.GetAllAsync(ct);
        var salesSummary = await salesService.GetDailySummaryAsync(null, ct);
        var preOrders = await salesService.GetPreOrdersAsync(ct);

        ViewData["Title"] = "Inventario Diario";
        ViewData["ActiveNav"] = "inventory";
        ViewBag.Products = products
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name })
            .ToList();
        ViewBag.SalesSummary = salesSummary;
        ViewBag.PreOrders = preOrders;

        return View(today);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        [FromServices] IProductionCaptureService captureService,
        [FromForm] List<DailyProductionLineRequest> lines,
        CancellationToken ct)
    {
        if (lines == null || lines.Count == 0)
        {
            Response.Headers["HX-Trigger"] = "{\"showToast\": \"Agregá al menos un producto.\"}";
            return NoContent();
        }

        await captureService.UpsertTodayAsync(new UpsertDailyProductionRequest(lines), ct);

        Response.Headers["HX-Trigger"] = "inventory-updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseDay(
        [FromServices] IProductionCaptureService captureService,
        CancellationToken ct)
    {
        await captureService.CloseTodayAsync(Guid.Empty, ct);
        Response.Headers["HX-Trigger"] = "inventory-updated";
        return RedirectToAction(nameof(Index));
    }
}
