using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Operations;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;

namespace Pana.Api.Web.Controllers;

/// <summary>
/// Unified daily register — the staff's operational hub.
/// Combines: inventory reconciliation, production tracking, waste/returns, and day closure.
/// Routes: /inventory (legacy) and /registro-diario (canonical).
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

        ViewData["Title"] = "Registro Diario";
        ViewData["ActiveNav"] = "registro-diario";
        ViewBag.Products = products
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name })
            .ToList();
        ViewBag.SalesSummary = salesSummary;

        return View("RegistroDiario", today);
    }

    [HttpPost("event")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(
        [FromServices] IProductionCaptureService captureService,
        [FromForm] AddProductionEventRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            await captureService.AddEventAsync(request, userId, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        [FromServices] IProductionCaptureService captureService,
        [FromForm] List<DailyProductionLineRequest> lines,
        CancellationToken ct)
    {
        if (lines == null || lines.Count == 0)
        {
            TempData["Error"] = "Agregá al menos un producto.";
            return RedirectToAction(nameof(Index));
        }

        await captureService.UpsertTodayAsync(new UpsertDailyProductionRequest(lines), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseDay(
        [FromServices] IProductionCaptureService captureService,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        try
        {
            await captureService.CloseTodayAsync(userId, ct);
            TempData["Success"] = "Día cerrado correctamente. El inventario fue actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
