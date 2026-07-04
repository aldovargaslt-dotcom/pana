using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class TabsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        List<TabItem> tabs,
        string activeTab = "")
    {
        if (string.IsNullOrEmpty(activeTab) && tabs.Count > 0)
            activeTab = tabs[0].Id;

        var props = new TabsProps(tabs, activeTab);
        return View(props);
    }
}
