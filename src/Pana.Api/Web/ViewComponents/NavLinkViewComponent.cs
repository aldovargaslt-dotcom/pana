using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class NavLinkViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string href, string label, string? icon = null, bool active = false)
    {
        ViewBag.Href = href;
        ViewBag.Label = label;
        ViewBag.Icon = icon;
        ViewBag.Active = active;
        return View();
    }
}
