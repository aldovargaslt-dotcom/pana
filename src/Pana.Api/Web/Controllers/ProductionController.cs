using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Operations;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Route("production")]
public class ProductionController : Controller
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
        var preOrders = await salesService.GetPreOrdersAsync(ct);

        ViewData["Title"] = "Producción Diaria";
        ViewBag.Products = products
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Name })
            .ToList();
        ViewBag.PreOrders = preOrders
            .Where(p => p.Status is "Confirmed" or "InProduction")
            .Select(p => new ScheduledOrderItemViewModel(
                p.Id,
                $"Pre-orden #{p.Id.ToString()[..8]}",
                p.CustomerName,
                p.CustomerPhone,
                p.CreatedAt,
                p.ScheduledDate,
                p.TotalAmount,
                p.DepositAmount,
                p.BalanceDue,
                p.Status,
                p.PaymentStatus,
                p.Items.Count
            ))
            .ToList();

        return View(today);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        [FromServices] IProductionCaptureService captureService,
        [FromServices] IProductService productService,
        [FromForm] List<DailyProductionLineRequest> lines,
        CancellationToken ct)
    {
        if (lines == null || lines.Count == 0)
        {
            Response.Headers["HX-Trigger"] = "{\"showToast\": \"Agregá al menos un producto.\"}";
            return NoContent();
        }

        await captureService.UpsertTodayAsync(new UpsertDailyProductionRequest(lines), ct);

        Response.Headers["HX-Trigger"] = "production-updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseDay(
        [FromServices] IProductionCaptureService captureService,
        CancellationToken ct)
    {
        await captureService.CloseTodayAsync(Guid.Empty, ct);
        Response.Headers["HX-Trigger"] = "production-updated";
        return RedirectToAction(nameof(Index));
    }
}
