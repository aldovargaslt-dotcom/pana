using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Common;
using Pana.Api.Application.Inventory;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize]
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

    // ── Recent Sales widget ──────────────────────────────────
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

    // ── KPI Drilldown ───────────────────────────────────────────

    [HttpGet("web/dashboard/widget/kpi-drilldown/{type}")]
    public async Task<IActionResult> KpiDrilldown(
        string type,
        [FromServices] ISalesService salesService,
        [FromServices] IProductService productService,
        [FromServices] IInventoryService inventoryService,
        CancellationToken ct)
    {
        object model = type switch
        {
            "revenue" => await BuildRevenueDrilldown(salesService, ct),
            "orders" => await BuildOrdersDrilldown(salesService, ct),
            "margin" => await BuildMarginDrilldown(salesService, ct),
            "products" => await BuildProductsDrilldown(productService, inventoryService, ct),
            _ => new KpiDrilldownViewModel("Desconocido", "metric", new List<DrilldownRowViewModel>(), "")
        };

        return PartialView("_KpiDrilldown", model);
    }

    private async Task<KpiDrilldownViewModel> BuildRevenueDrilldown(ISalesService salesService, CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var rows = sales.Take(5).Select(s => new DrilldownRowViewModel(
            s.Id,
            s.TotalAmount.ToString("C0"),
            s.Items.Count + " art.",
            s.CreatedAt,
            s.Status
        )).ToList();

        return new KpiDrilldownViewModel("Ventas recientes", "revenue", rows, "/sales");
    }

    private async Task<KpiDrilldownViewModel> BuildOrdersDrilldown(ISalesService salesService, CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var rows = sales.Take(5).Select(s => new DrilldownRowViewModel(
            s.Id,
            "#" + sales.Take(5).ToList().IndexOf(s),
            s.Items.Count + " artículo" + (s.Items.Count != 1 ? "s" : ""),
            s.CreatedAt,
            s.Status
        )).ToList();

        // Fix order numbering
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i] = rows[i] with { Primary = "Ord. " + (i + 1) };
        }

        return new KpiDrilldownViewModel("Órdenes recientes", "orders", rows, "/sales");
    }

    private async Task<KpiDrilldownViewModel> BuildMarginDrilldown(ISalesService salesService, CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var rows = sales.Take(5).Select(s =>
        {
            var itemCount = s.Items.Count;
            var avgPerItem = itemCount > 0 ? s.TotalAmount / itemCount : 0;
            return new DrilldownRowViewModel(
                s.Id,
                s.TotalAmount.ToString("C0"),
                "~" + avgPerItem.ToString("C0") + "/art.",
                s.CreatedAt,
                s.Status
            );
        }).ToList();

        return new KpiDrilldownViewModel("Margen reciente", "margin", rows, "/sales");
    }

    private async Task<KpiDrilldownViewModel> BuildProductsDrilldown(
        IProductService productService, IInventoryService inventoryService, CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var stockLevels = await inventoryService.GetStockLevelsAsync(ct);
        var stockMap = stockLevels.ToDictionary(s => s.ProductId, s => s.CurrentStock);

        var rows = products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p =>
            {
                var stock = stockMap.TryGetValue(p.Id, out var s) ? s : 0;
                return new DrilldownRowViewModel(
                    p.Id,
                    p.Name,
                    stock + " en stock",
                    p.CreatedAt,
                    stock <= 5 ? "Bajo" : "OK"
                );
            })
            .ToList();

        return new KpiDrilldownViewModel("Productos recientes", "products", rows, "/products");
    }
}
