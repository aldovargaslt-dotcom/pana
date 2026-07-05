using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

/// <summary>
/// Renders the Activity module's sub-navigation sidebar with three tabs:
/// BillingQueue, ScheduledOrders, and OrderHistory.
/// The active tab is determined by the current route.
/// </summary>
public class ActivitySidebarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var currentPath = HttpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
        var activeTab = currentPath switch
        {
            string p when p.Contains("/scheduled-orders") => "scheduled",
            string p when p.Contains("/order-history") => "history",
            _ => "billing"
        };

        ViewBag.ActiveTab = activeTab;
        return View();
    }
}
