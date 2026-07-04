using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class CardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string title, string? subtitle = null, string? hxGet = null)
    {
        ViewBag.Title = title;
        ViewBag.Subtitle = subtitle;
        ViewBag.HxGet = hxGet;
        return View();
    }
}
