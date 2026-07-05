using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Common;
using Pana.Api.Application.Inventory;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Route("")]
public class DashboardController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IProductService productService,
        [FromServices] ISalesService salesService,
        [FromServices] IInventoryService inventoryService,
        [FromServices] IDailyContextService dailyContextService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var todaySummary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date, ct: ct);
        var yesterdaySummary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date.AddDays(-1), ct: ct);
        var stockLevels = await inventoryService.GetStockLevelsAsync(ct);

        var lowStockCount = stockLevels.Count(s => s.CurrentStock <= 5); // threshold

        var kpi = new DashboardKpiViewModel(
            TodayRevenue: todaySummary.TotalRevenue,
            TodayOrderCount: todaySummary.SaleCount,
            TodayMargin: todaySummary.TotalMargin,
            YesterdayRevenue: yesterdaySummary.TotalRevenue,
            YesterdayOrderCount: yesterdaySummary.SaleCount,
            ActiveProductCount: products.Count(p => p.IsActive),
            LowStockCount: lowStockCount
        );

        ViewData["Title"] = "Dashboard";
        ViewData["ActiveNav"] = "dashboard";

        var vm = new DashboardViewModel(
            TenantName: "Default Bakery",
            Widgets:
            [
                new("kpi-cards", "kpi_cards", "KPIs",
                    "/web/dashboard/widget/kpi-cards", 4),
                new("daily-context", "daily_context", "Contexto del día",
                    "/web/dashboard/widget/daily-context", 4),
                new("sales-trend", "sales_chart", "Tendencia de ventas",
                    "/web/dashboard/widget/sales-trend", 3),
                new("recent-sales", "data_table", "Últimas ventas",
                    "/web/dashboard/widget/recent-sales", 2),
                new("low-stock", "data_table", "Stock bajo",
                    "/web/dashboard/widget/low-stock", 2),
            ],
            Kpi: kpi
        );

        return View(vm);
    }

    // ── KPI Cards widget ──────────────────────────────────────

    [HttpGet("web/dashboard/widget/kpi-cards")]
    public async Task<IActionResult> KpiCardsWidget(
        [FromServices] IProductService productService,
        [FromServices] ISalesService salesService,
        [FromServices] IInventoryService inventoryService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var todaySummary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date, ct: ct);
        var yesterdaySummary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date.AddDays(-1), ct: ct);
        var stockLevels = await inventoryService.GetStockLevelsAsync(ct);
        var lowStockCount = stockLevels.Count(s => s.CurrentStock <= 5);

        var kpi = new DashboardKpiViewModel(
            TodayRevenue: todaySummary.TotalRevenue,
            TodayOrderCount: todaySummary.SaleCount,
            TodayMargin: todaySummary.TotalMargin,
            YesterdayRevenue: yesterdaySummary.TotalRevenue,
            YesterdayOrderCount: yesterdaySummary.SaleCount,
            ActiveProductCount: products.Count(p => p.IsActive),
            LowStockCount: lowStockCount
        );

        return PartialView("_KpiCards", kpi);
    }

    // ── Daily Context widget ──────────────────────────────────

    [HttpGet("web/dashboard/widget/daily-context")]
    public async Task<IActionResult> DailyContextWidget(
        [FromServices] IDailyContextService dailyContextService,
        CancellationToken ct)
    {
        var context = await dailyContextService.GetTodayAsync(ct);
        return PartialView("_DailyContextBar", context);
    }

    // ── Sales Trend widget ────────────────────────────────────

    [HttpGet("web/dashboard/widget/sales-trend")]
    public async Task<IActionResult> SalesTrendWidget(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var points = new List<Application.Analytics.DailyTrendPointDto>();
        var culture = new System.Globalization.CultureInfo("es-MX");

        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var summary = await salesService.GetDailySummaryAsync(date: date, ct: ct);
            points.Add(new Application.Analytics.DailyTrendPointDto(
                Date: date,
                Label: culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek),
                Ventas: summary.TotalRevenue,
                Transacciones: summary.SaleCount,
                TicketPromedio: summary.SaleCount > 0 ? summary.TotalRevenue / summary.SaleCount : 0
            ));
        }

        return PartialView("_SalesTrendChart", points);
    }

    // ── Legacy / existing widget endpoints ────────────────────

    [HttpGet("web/dashboard/widget/sales-chart")]
    public async Task<IActionResult> SalesChartWidget(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var summary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date, ct: ct);
        var data = new List<SalesChartData>
        {
            new(DateTime.UtcNow.ToString("ddd"), summary.TotalRevenue)
        };
        return PartialView("_SalesChartWidget", data);
    }

    [HttpGet("web/dashboard/widget/product-count")]
    public async Task<IActionResult> ProductCountWidget(
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        return PartialView("_ProductCountWidget", products.Count);
    }

    [HttpGet("web/dashboard/widget/recent-sales")]
    public async Task<IActionResult> RecentSalesWidget(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var recent = sales.Take(5).Select(s => new SaleRowViewModel(
            s.Id,
            s.Status,
            s.TotalAmount,
            s.Items.Count,
            s.CreatedAt
        )).ToList();

        return PartialView("_RecentSalesWidget", recent);
    }

    [HttpGet("web/dashboard/widget/low-stock")]
    public async Task<IActionResult> LowStockWidget(
        [FromServices] IInventoryService inventoryService,
        CancellationToken ct)
    {
        var stockLevels = await inventoryService.GetStockLevelsAsync(ct);
        var lowStock = stockLevels
            .Where(s => s.CurrentStock <= 10)
            .OrderBy(s => s.CurrentStock)
            .Take(10)
            .Select(s => new LowStockAlertViewModel(
                s.ProductId,
                s.ProductName,
                s.CurrentStock,
                MinimumLevel: 5 // threshold — ideally from ReorderRule
            ))
            .ToList();

        return PartialView("_LowStockWidget", lowStock);
    }
}
