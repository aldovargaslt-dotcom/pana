# Pana View Components Reference

All 8 View Components available in `src/Pana.Api/Web/Components/`. Use these instead of raw HTML for consistency and HTMX integration.

## Modal

**File**: `Web/Components/Modal/ModalProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Id` | `string` | Yes | Unique modal ID |
| `Title` | `string` | Yes | Modal heading |
| `HxGet` | `string?` | No | URL to load modal content via HTMX GET |
| `HxPost` | `string?` | No | URL for form submission via HTMX POST |
| `Size` | `ModalSize` | No | `Small`, `Medium` (default), `Large` |

### Usage

```html
<!-- Trigger -->
<button hx-get="/products/create-form" hx-target="#modal-container">New Product</button>

<!-- The controller returns a partial that includes the Modal component -->
@* In _CreateForm.cshtml *@
@await Component.InvokeAsync("Modal", new {
    Id = "create-product-modal",
    Title = "New Product",
    HxPost = "/products/create"
})
<!-- Form fields inside modal body -->
```

### HTMX Pattern
- Trigger button uses `hx-get` pointing to a controller action that returns a partial view
- The partial view wraps content in the Modal component
- Form inside modal uses `hx-post`, target is the table body (not the modal)
- On success (`hx-on::after-request`), close the modal by removing its DOM element

## DataTable

**File**: `Web/Components/DataTable/DataTableProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Id` | `string` | Yes | Table container ID |
| `HxGet` | `string` | Yes | URL for loading/searching rows |
| `Columns` | `List<ColumnDef>` | Yes | Column definitions |
| `SearchPlaceholder` | `string` | No | Search input placeholder text |
| `CreateButtonText` | `string?` | No | Text for create button |
| `CreateHxGet` | `string?` | No | URL for create form modal |
| `EmptyMessage` | `string` | No | Message when no rows |

### Usage

```html
@await Component.InvokeAsync("DataTable", new {
    Id = "products-table",
    HxGet = "/products/table-rows",
    Columns = new List<ColumnDef> {
        new("Name", "Nombre"),
        new("Price", "Precio"),
        new("Category", "Categoría")
    },
    SearchPlaceholder = "Search products...",
    CreateButtonText = "New Product",
    CreateHxGet = "/products/create-form"
})
```

### HTMX Pattern
- Search input triggers `hx-get` with `keyup changed delay:300ms`
- Table body refreshes after create/update/delete via custom events
- Controller returns partial view with just `<tr>` elements

## StatCard

**File**: `Web/Components/StatCard/StatCardProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Title` | `string` | Yes | Metric name |
| `Value` | `string` | Yes | Current value |
| `ChangePct` | `decimal?` | No | Percentage change (positive = green, negative = red) |
| `ChangeLabel` | `string?` | No | Context label for change (e.g., "vs last month") |
| `HxGet` | `string?` | No | URL for auto-refresh polling |
| `Icon` | `string?` | No | Icon class or emoji |

### Usage

```html
@await Component.InvokeAsync("StatCard", new {
    Title = "Today's Sales",
    Value = "$1,245.00",
    ChangePct = 12.5m,
    ChangeLabel = "vs yesterday",
    HxGet = "/dashboard/today-sales-stat",
    Icon = "💰"
})
```

### HTMX Pattern
- Automatically polls with `hx-trigger="every 60s"` when `HxGet` is provided
- Server returns a new StatCard partial with updated values
- Keep the container `id` stable for smooth CSS transitions

## Button

**File**: `Web/Components/Button/ButtonProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Text` | `string` | Yes | Button label |
| `Variant` | `ButtonVariant` | No | `Primary` (default), `Secondary`, `Danger`, `Ghost` |
| `Size` | `ButtonSize` | No | `Small`, `Medium` (default), `Large` |
| `Type` | `string` | No | `button` (default), `submit` |
| `HxGet` | `string?` | No | HTMX GET URL |
| `HxPost` | `string?` | No | HTMX POST URL |
| `HxDelete` | `string?` | No | HTMX DELETE URL |
| `HxTarget` | `string?` | No | HTMX target selector |
| `HxConfirm` | `string?` | No | Confirmation message |
| `Icon` | `string?` | No | Icon class or emoji |
| `Disabled` | `bool` | No | Disabled state |

### Usage

```html
@await Component.InvokeAsync("Button", new {
    Text = "Save",
    Variant = ButtonVariant.Primary,
    Type = "submit"
})

@await Component.InvokeAsync("Button", new {
    Text = "Delete",
    Variant = ButtonVariant.Danger,
    HxDelete = $"/products/{Model.Id}",
    HxTarget = $"#product-row-{Model.Id}",
    HxConfirm = "Delete this product?",
    Icon = "🗑️"
})
```

## Card

**File**: `Web/Components/Card/CardProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `HxGet` | `string?` | No | Make entire card clickable via HTMX GET |
| `HxTrigger` | `string?` | No | HTMX trigger for the card request |

### Usage

```html
@await Component.InvokeAsync("Card", new {
    HxGet = $"/products/{Model.Id}/detail",
    HxTrigger = "click"
})
{
    <!-- Card content as child content -->
    <h3>@Model.Name</h3>
    <p>@Model.Price</p>
}
```

### HTMX Pattern
- When `HxGet` is set, the card becomes clickable and loads content via HTMX
- Card content is rendered as child content (Razor `RenderFragment`)

## EmptyState

**File**: `Web/Components/EmptyState/EmptyStateProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Title` | `string` | Yes | Empty state heading |
| `Message` | `string` | Yes | Descriptive message |
| `ActionText` | `string?` | No | Call-to-action button text |
| `ActionHxGet` | `string?` | No | URL for CTA button |
| `ActionHxTarget` | `string?` | No | Target for CTA button |
| `Icon` | `string` | No | Emoji or icon (default: 📦) |

### Usage

```html
@await Component.InvokeAsync("EmptyState", new {
    Title = "No recipes found",
    Message = "Create your first recipe to start tracking production costs.",
    ActionText = "New Recipe",
    ActionHxGet = "/recipes/create-form",
    ActionHxTarget = "#modal-container"
})
```

## Tabs

**File**: `Web/Components/Tabs/TabsProps.cs`

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Tabs` | `List<TabDef>` | Yes | Tab definitions (label, URL, active) |
| `ActiveTab` | `string` | Yes | ID of the active tab |

### Usage

```html
@await Component.InvokeAsync("Tabs", new {
    Tabs = new List<TabDef> {
        new("details", "Details", $"/products/{Model.Id}/details"),
        new("history", "History", $"/products/{Model.Id}/history"),
        new("analytics", "Analytics", $"/products/{Model.Id}/analytics")
    },
    ActiveTab = "details"
})
```

### HTMX Pattern
- Each tab uses `hx-get` to load its pane content
- Active tab is highlighted with brand color styles
- Pane content is loaded lazily on first click

## NavLink

**File**: `Web/Components/NavLink/` (dedicated component)

### Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `Href` | `string` | Yes | Navigation URL |
| `Icon` | `string` | Yes | Icon class or SVG |
| `Text` | `string` | Yes | Link label |
| `IsActive` | `bool` | No | Whether this is the current page |

### Usage

```html
@await Component.InvokeAsync("NavLink", new {
    Href = "/products",
    Icon = "📦",
    Text = "Products",
    IsActive = Context.Request.Path.StartsWithSegments("/products")
})
```

### Pattern
- Automatically highlights when `IsActive` is true
- Used in the sidebar navigation in `_Layout.cshtml`
- Routes to Razor Pages / MVC controller actions

---

## Component Composition Example

Putting it all together for a typical list page:

```html
@* Views/Products/Index.cshtml *@
@model ProductsIndexViewModel
@{
    ViewData["Title"] = "Productos";
}

<div class="fade-in space-y-6">
    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Productos</h1>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                @Model.TotalCount productos en catálogo
            </p>
        </div>
        <button hx-get="/products/create-form" hx-target="#modal-container"
                class="inline-flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 ...">
            + Nuevo Producto
        </button>
    </div>

    <!-- Stat Cards Row -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @await Component.InvokeAsync("StatCard", new {
            Title = "Total Products", Value = Model.TotalCount.ToString(), ...
        })
        <!-- More stat cards... -->
    </div>

    <!-- Data Table -->
    @await Component.InvokeAsync("DataTable", new {
        Id = "products-table",
        HxGet = "/products/table-rows",
        Columns = Model.Columns,
        SearchPlaceholder = "Buscar productos...",
        CreateButtonText = "Nuevo Producto",
        CreateHxGet = "/products/create-form"
    })

    <!-- Empty State (shown when no results) -->
    @if (!Model.HasProducts)
    {
        @await Component.InvokeAsync("EmptyState", new {
            Title = "No hay productos",
            Message = "Agrega tu primer producto al catálogo.",
            ActionText = "Nuevo Producto",
            ActionHxGet = "/products/create-form",
            ActionHxTarget = "#modal-container"
        })
    }
</div>
```
