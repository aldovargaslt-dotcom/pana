using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string label, string? url = null, string? hxGet = null, string? hxPost = null, string? cssClass = null)
    {
        ViewBag.Label = label;
        ViewBag.Url = url;
        ViewBag.HxGet = hxGet;
        ViewBag.HxPost = hxPost;
        ViewBag.CssClass = cssClass ?? "";
        return View();
    }
}
