using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Route("activity")]
public class ActivityController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> BillingQueue(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);

        var queueItems = sales.Select(s => new BillingQueueItemViewModel(
            s.Id,
            $"Venta #{s.Id.ToString()[..8]}",
            null, // TableNumber — integration point
            s.CreatedAt,
            s.TotalAmount,
            s.Status
        )).ToList();

        var activeCount = queueItems.Count(q => q.Status is "Draft" or "Confirmed" or "Preparing" or "Ready");
        var closedCount = queueItems.Count(q => q.Status is "Completed" or "Voided");

        ViewData["Title"] = "Actividad — Cola de Facturación";
        ViewData["ActiveNav"] = "activity";

        var vm = new ActivityViewModel(
            "billing-queue", queueItems, new(), activeCount, closedCount);
        return View("BillingQueue", vm);
    }

    [HttpGet("order-history")]
    public async Task<IActionResult> OrderHistory(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);

        var historyItems = sales.Select((s, i) => new OrderHistoryItemViewModel(
            s.Id,
            i + 1,
            s.CreatedAt,
            $"Venta #{s.Id.ToString()[..8]}",
            s.Status,
            s.TotalAmount,
            s.Status switch
            {
                "Completed" => "Pagado",
                "Voided" => "Anulado",
                _ => "Pendiente"
            }
        )).ToList();

        var queueItems = sales.Select(s => new BillingQueueItemViewModel(
            s.Id,
            $"Venta #{s.Id.ToString()[..8]}",
            null,
            s.CreatedAt,
            s.TotalAmount,
            s.Status
        )).ToList();

        var activeCount = queueItems.Count(q => q.Status is "Draft" or "Confirmed" or "Preparing" or "Ready");
        var closedCount = queueItems.Count(q => q.Status is "Completed" or "Voided");

        ViewData["Title"] = "Actividad — Historial de Órdenes";
        ViewData["ActiveNav"] = "activity";

        var vm = new ActivityViewModel(
            "order-history", queueItems, historyItems, activeCount, closedCount);
        return View("OrderHistory", vm);
    }
}
