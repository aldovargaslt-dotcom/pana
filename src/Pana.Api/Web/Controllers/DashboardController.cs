using Microsoft.AspNetCore.Mvc;
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
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var summary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date, ct: ct);

        ViewData["Title"] = "Dashboard";
        ViewData["ActiveNav"] = "dashboard";

        var vm = new DashboardViewModel(
            TenantName: "Default Bakery",
            Widgets:
            [
                new("sales-today", "sales_chart", "Ventas del día",
                    "/web/dashboard/widget/sales-chart", 3),
                new("product-count", "stat_card", "Productos",
                    "/web/dashboard/widget/product-count", 1),
                new("recent-sales", "data_table", "Últimas ventas",
                    "/web/dashboard/widget/recent-sales", 2),
                new("low-stock", "data_table", "Stock bajo",
                    "/web/dashboard/widget/low-stock", 2),
            ]
        );

        return View(vm);
    }

    [HttpGet("web/dashboard/widget/sales-chart")]
    public async Task<IActionResult> SalesChartWidget(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var summary = await salesService.GetDailySummaryAsync(date: DateTime.UtcNow.Date, ct: ct);

        // Simple chart: just today for now
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
        // Return just the stat card partial
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
    public IActionResult LowStockWidget()
    {
        // Placeholder — depends on inventory module
        return PartialView("_LowStockWidget", Array.Empty<object>());
    }
}
