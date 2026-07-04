using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class StatCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string label, string value, string? trend = null,
        string? trendValue = null, string? icon = null, string? hxGet = null)
    {
        ViewBag.Label = label;
        ViewBag.Value = value;
        ViewBag.Trend = trend;
        ViewBag.TrendValue = trendValue;
        ViewBag.Icon = icon ?? "box";
        ViewBag.HxGet = hxGet;
        return View();
    }
}
