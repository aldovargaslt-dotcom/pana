using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class TabsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string? tabs = null)
    {
        ViewBag.Tabs = tabs;
        return View();
    }
}
