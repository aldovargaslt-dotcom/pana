using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class StatCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string label,
        string value,
        string? trend = null,
        string? trendValue = null,
        string? icon = null,
        string? hxGet = null)
    {
        var props = new StatCardProps(label, value, trend, trendValue, icon, hxGet);
        return View(props);
    }
}
