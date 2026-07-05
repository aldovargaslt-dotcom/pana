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
            s.CustomerName,
            null, // TableNumber — integration point
            s.CreatedAt,
            s.TotalAmount,
            s.Status,
            s.OrderType,
            s.PaymentStatus
        )).ToList();

        var activeCount = queueItems.Count(q => q.Status is "Draft" or "Confirmed" or "InProduction" or "Ready");
        var closedCount = queueItems.Count(q => q.Status is "Delivered" or "Cancelled");

        ViewData["Title"] = "Actividad — Cola de Facturación";
        ViewData["ActiveNav"] = "activity";

        var vm = new ActivityViewModel(
            "billing-queue", queueItems, new(), new(), activeCount, closedCount);
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
            s.CustomerName is not null ? $"{s.CustomerName} — #{s.Id.ToString()[..8]}" : $"Venta #{s.Id.ToString()[..8]}",
            s.Status,
            s.TotalAmount,
            s.PaymentStatus,
            s.OrderType,
            s.CustomerName
        )).ToList();

        var queueItems = sales.Select(s => new BillingQueueItemViewModel(
            s.Id,
            $"Venta #{s.Id.ToString()[..8]}",
            s.CustomerName,
            null,
            s.CreatedAt,
            s.TotalAmount,
            s.Status,
            s.OrderType,
            s.PaymentStatus
        )).ToList();

        var activeCount = queueItems.Count(q => q.Status is "Draft" or "Confirmed" or "InProduction" or "Ready");
        var closedCount = queueItems.Count(q => q.Status is "Delivered" or "Cancelled");

        ViewData["Title"] = "Actividad — Historial de Órdenes";
        ViewData["ActiveNav"] = "activity";

        var vm = new ActivityViewModel(
            "order-history", queueItems, historyItems, new(), activeCount, closedCount);
        return View("OrderHistory", vm);
    }

    [HttpGet("scheduled-orders")]
    public async Task<IActionResult> ScheduledOrders(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var preOrders = await salesService.GetPreOrdersAsync(ct);

        var scheduledItems = preOrders.Select(s => new ScheduledOrderItemViewModel(
            s.Id,
            $"Pre-orden #{s.Id.ToString()[..8]}",
            s.CustomerName,
            s.CustomerPhone,
            s.CreatedAt,
            s.ScheduledDate,
            s.TotalAmount,
            s.DepositAmount,
            s.BalanceDue,
            s.Status,
            s.PaymentStatus,
            s.Items.Count
        )).ToList();

        ViewData["Title"] = "Actividad — Pre-órdenes";
        ViewData["ActiveNav"] = "activity";

        var vm = new ActivityViewModel(
            "scheduled-orders",
            new(),
            new(),
            scheduledItems,
            scheduledItems.Count,
            0
        );
        return View("ScheduledOrders", vm);
    }
}
