using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class CardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string? title = null,
        string? subtitle = null,
        string? hxGet = null,
        string? hxTrigger = "load",
        string padding = "md")
    {
        var props = new CardProps(title, subtitle, hxGet, hxTrigger, padding);
        return View(props);
    }
}
