namespace Pana.Api.Web.Components;

public record TabsProps(
    List<TabItem> Tabs,
    string ActiveTab = ""
);

public record TabItem(
    string Id,
    string Label,
    string HxGet,
    string HxTarget
);
