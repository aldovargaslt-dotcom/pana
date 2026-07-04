namespace Pana.Api.Web.Components;

public record DataTableProps(
    List<DataTableColumn> Columns,
    string? HxGet = null,
    string? EmptyMessage = "No hay datos disponibles.",
    bool Searchable = true
);

public record DataTableColumn(
    string Key,
    string Label,
    bool Sortable = false,
    string? Width = null
);
