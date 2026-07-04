using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class DataTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string id, string? hxGet = null, string? hxTrigger = null)
    {
        ViewBag.Id = id;
        ViewBag.HxGet = hxGet;
        ViewBag.HxTrigger = hxTrigger ?? "load";
        return View();
    }
}
