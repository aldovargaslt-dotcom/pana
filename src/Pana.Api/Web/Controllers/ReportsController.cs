using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Analytics;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("reports")]
public class ReportsController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ISalesService salesService,
        [FromServices] IProductService productService,
        [FromQuery] string? period = "today",
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var (from, to, periodLabel) = period switch
        {
            "yesterday" => (now.Date.AddDays(-1), now.Date.AddDays(-1).AddDays(1).AddTicks(-1), "Ayer"),
            "week" => (now.Date.AddDays(-7), now, "Últimos 7 días"),
            "month" => (now.Date.AddDays(-30), now, "Últimos 30 días"),
            _ => (now.Date, now, "Hoy")
        };

        // Get analytics data
        var trends = await analyticsService.GetSalesTrendsAsync(from, to, ct);
        var sales = await salesService.GetAllAsync(ct);

        // Filter sales to period
        var periodSales = sales
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
            .ToList();

        var completedSales = periodSales.Where(s => s.Status == "Delivered").ToList();

        // Previous period for growth comparison
        var prevFrom = from.AddDays(-(to - from).Days - 1);
        var prevTo = from.AddTicks(-1);
        var prevTrends = await analyticsService.GetSalesTrendsAsync(prevFrom, prevTo, ct);

        var prevTotalSales = prevTrends.VentasTotales;
        var prevTotalTrans = prevTrends.TotalTransacciones;

        var salesGrowth = prevTotalSales > 0
            ? ((trends.VentasTotales - prevTotalSales) / prevTotalSales) * 100
            : 0m;
        var transGrowth = prevTotalTrans > 0
            ? ((decimal)(trends.TotalTransacciones - prevTotalTrans) / prevTotalTrans) * 100
            : 0m;

        var metrics = new ReportsMetricsViewModel(
            TotalSales: trends.VentasTotales,
            TotalTransactions: trends.TotalTransacciones,
            TotalCustomers: periodSales.Select(s => s.Id).Distinct().Count(), // proxy
            NetProfit: 0, // Integration point — P&L data not yet available via direct service
            SalesGrowthPct: Math.Round(salesGrowth, 1),
            TransactionGrowthPct: Math.Round(transGrowth, 1),
            CustomerGrowthPct: 0, // Integration point
            ProfitGrowthPct: 0  // Integration point
        );

        var dailyTrend = trends.DailyTrend.Select(d => new DailyTrendPointViewModel(
            d.Date, d.Label, d.Ventas, d.Transacciones
        )).ToList();

        // Top products from completed sales
        var productSalesMap = completedSales
            .SelectMany(s => s.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(i => i.Quantity), TotalOrders = g.Count() })
            .OrderByDescending(x => x.TotalQty)
            .Take(6)
            .ToList();

        var productIds = productSalesMap.Select(x => x.ProductId).ToList();
        var allProducts = await productService.GetAllAsync(ct);
        var productDict = allProducts.ToDictionary(p => p.Id);

        var topProducts = productSalesMap.Select(x =>
        {
            productDict.TryGetValue(x.ProductId, out var p);
            return new TopProductViewModel(
                x.ProductId,
                p?.Name ?? "Producto desconocido",
                p?.ProductType ?? "",
                x.TotalOrders,
                x.TotalQty
            );
        }).ToList();

        var allOrders = sales.Select((s, i) => new OrderHistoryItemViewModel(
            s.Id, i + 1, s.CreatedAt,
            s.CustomerName is not null ? $"{s.CustomerName} — #{s.Id.ToString()[..8]}" : $"Venta #{s.Id.ToString()[..8]}",
            s.Status,
            s.TotalAmount,
            s.PaymentStatus,
            s.OrderType,
            s.CustomerName
        )).ToList();

        ViewData["Title"] = "Reportes";
        ViewData["ActiveNav"] = "reports";

        // BCG Matrix (last 30 days)
        var bcgFrom = now.Date.AddDays(-30);
        var bcgTo = now;
        var bcg = await analyticsService.GetBcgMatrixAsync(bcgFrom, bcgTo, ct);
        var bcgProducts = bcg.Products.Select(p => new BcgProductViewModel(
            p.ProductId, p.ProductName, p.UnitsSold, p.Revenue,
            p.Quadrant, p.Strategy, BcgProductViewModel.GetIcon(p.Quadrant))).ToList();

        var vm = new ReportsViewModel(
            periodLabel, from, to, metrics, dailyTrend, topProducts, allOrders, bcgProducts);
        return View(vm);
    }
}
