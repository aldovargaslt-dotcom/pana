using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class DataTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        List<DataTableColumn> columns,
        string? hxGet = null,
        string? emptyMessage = "No hay datos disponibles.",
        bool searchable = true)
    {
        var props = new DataTableProps(columns, hxGet, emptyMessage, searchable);
        return View(props);
    }
}
