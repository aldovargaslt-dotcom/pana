using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class ModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string id, string title, string? content = null)
    {
        ViewBag.Id = id;
        ViewBag.Title = title;
        ViewBag.Content = content;
        return View();
    }
}
