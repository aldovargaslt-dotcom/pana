using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class ModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string id,
        string title,
        string? hxGet = null,
        string? hxPost = null,
        string size = "md")
    {
        var props = new ModalProps(id, title, hxGet, hxPost, size);
        return View(props);
    }
}
