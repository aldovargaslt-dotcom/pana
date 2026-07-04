using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class EmptyStateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string title,
        string description,
        string? actionLabel = null,
        string? actionHxGet = null,
        string? actionHxTarget = "#modal-container")
    {
        var props = new EmptyStateProps(title, description, actionLabel, actionHxGet, actionHxTarget);
        return View(props);
    }
}
