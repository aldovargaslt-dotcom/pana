# Pana UI Patterns — HTMX + Razor Catalog

> **Reusable patterns for building Pana views.**
> Always reference this before creating new `.cshtml` files.
> Design system: `design/DESIGN.md` | Tokens: `wwwroot/css/pana-tokens.css`

---

## Pattern 1: Page Shell

Every new page follows this structure:

```html
@{
    ViewData["Title"] = "Page Title";
    Layout = "_Layout";
}

<div class="page-container">
    <!-- Page Header -->
    <header class="page-header">
        <h1 class="display">Page Title</h1>
        <p class="body-large muted">Optional subtitle or description.</p>
    </header>

    <!-- Main Content -->
    <main class="page-content">
        <!-- Content here -->
    </main>
</div>
```

**Rules:**
- Always set `ViewData["Title"]` — used by `_Layout` for `<title>` tag
- Use semantic HTML: `<header>`, `<main>`, `<section>`, `<article>`
- `page-container` → `page-header` → `page-content` hierarchy

---

## Pattern 2: Data Table

```html
<div class="surface-container rounded-lg overflow-hidden">
    <table class="w-full">
        <thead class="surface-container-high">
            <tr>
                <th class="text-left p-4 label">Column</th>
                <th class="text-right p-4 label">Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model.Items)
            {
                <tr class="border-t border-surface-container-high">
                    <td class="p-4 body">@item.Name</td>
                    <td class="p-4 text-right">
                        <a hx-get="/items/@item.Id/edit"
                           hx-target="#modal-container"
                           class="btn-ghost">
                            Editar
                        </a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

**Rules:**
- Use surface tonal layers, NOT 1px borders
- Rows separated by `border-t border-surface-container-high`
- Actions on the right, use `btn-ghost` for row-level actions
- Always use `overflow-hidden` on the container for rounded corners to clip

---

## Pattern 3: Modal (HTMX)

```html
<!-- Trigger (any page) -->
<button hx-get="/items/create"
        hx-target="#modal-container"
        hx-swap="innerHTML"
        class="btn-primary">
    + Nuevo Item
</button>

<!-- Modal container (in _Layout.cshtml, exists once) -->
<div id="modal-container"></div>

<!-- Modal partial (returned by /items/create) -->
<div class="modal-overlay" _="on click if event.target == me remove me">
    <div class="modal-content surface-container-lowest rounded-xl p-6"
         style="max-width: 480px; width: 90vw;">
        <header class="flex justify-between items-center mb-4">
            <h2 class="headline">Nuevo Item</h2>
            <button class="btn-icon" _="on click remove closest .modal-overlay">
                ✕
            </button>
        </header>

        <form hx-post="/items"
              hx-target="#items-table"
              hx-swap="outerHTML">
            @Html.AntiForgeryToken()
            <!-- Form fields -->
            <footer class="flex justify-end gap-3 mt-6">
                <button type="button"
                        class="btn-secondary"
                        _="on click remove closest .modal-overlay">
                    Cancelar
                </button>
                <button type="submit" class="btn-primary">
                    Guardar
                </button>
            </footer>
        </form>
    </div>
</div>
```

**Rules:**
- Modal trigger uses `hx-target="#modal-container"`
- Modal partial includes overlay + content
- Overlay closes on background click (Hyperscript: `_="on click if event.target == me remove me"`)
- Always include `@Html.AntiForgeryToken()` in POST forms
- Submit button has `type="submit"`, cancel has `type="button"`
- Use `surface-container-lowest` for modal card

---

## Pattern 4: Form (Inline / Page)

```html
<form hx-post="/items"
      hx-target="#item-list"
      hx-swap="afterbegin"
      hx-on::after-request="if(event.detail.successful) this.reset()">
    @Html.AntiForgeryToken()

    <div class="space-y-4">
        <!-- Text input -->
        <div class="form-group">
            <label for="name" class="label block mb-1">Nombre</label>
            <input type="text"
                   id="name"
                   name="name"
                   required
                   class="input w-full"
                   placeholder="Ej. Concha de vainilla" />
            <span class="text-danger text-sm validation-message"
                  data-valmsg-for="name"></span>
        </div>

        <!-- Select -->
        <div class="form-group">
            <label for="category" class="label block mb-1">Categoría</label>
            <select id="category"
                    name="category"
                    class="input w-full">
                <option value="">Seleccionar...</option>
                <option value="bread">Pan</option>
                <option value="pastry">Pastelería</option>
            </select>
        </div>

        <!-- Number input -->
        <div class="form-group">
            <label for="price" class="label block mb-1">Precio</label>
            <input type="number"
                   id="price"
                   name="price"
                   step="0.01"
                   min="0"
                   class="input w-full" />
        </div>
    </div>

    <div class="flex justify-end gap-3 mt-6">
        <button type="submit" class="btn-primary">
            Guardar
        </button>
    </div>
</form>
```

**Rules:**
- Every `<input>` has a `<label>` (use `for` + `id`)
- `hx-on::after-request` resets form on success
- Use `space-y-4` for vertical spacing between fields
- Validation messages use `data-valmsg-for` attribute
- Always include `@Html.AntiForgeryToken()`

---

## Pattern 5: Empty State

```html
<div class="empty-state text-center py-16">
    <div class="text-5xl mb-4">📦</div>
    <h3 class="headline muted mb-2">No hay productos todavía</h3>
    <p class="body muted mb-6">Crea tu primer producto para empezar a vender.</p>
    <button hx-get="/products/create"
            hx-target="#modal-container"
            class="btn-primary">
        + Crear Producto
    </button>
</div>
```

**Rules:**
- Always show empty state instead of blank page
- Include CTA button to create first item
- Use muted colors — empty state shouldn't compete with content

---

## Pattern 6: Toast / Notification (HTMX)

```html
<!-- Toast container (in _Layout.cshtml) -->
<div id="toast-container" class="fixed bottom-4 right-4 z-50 space-y-2"></div>

<!-- Toast partial (returned by server after action) -->
<div class="toast surface-container-highest rounded-lg p-4 shadow-lg flex items-center gap-3"
     _="on load wait 3s then transition opacity to 0 over 0.5s then remove me">
    <span class="text-success text-lg">✓</span>
    <p class="body">Producto guardado correctamente.</p>
</div>
```

**Rules:**
- Toast auto-dismisses after 3s (Hyperscript)
- Success = green icon + neutral card
- Error = red icon + neutral card
- Position: bottom-right, fixed

---

## Pattern 7: Button Hierarchy

```html
<!-- Primary CTA — gradient signature (135°, oxide red) -->
<button class="btn-primary">Guardar</button>

<!-- Secondary — tonal surface -->
<button class="btn-secondary">Cancelar</button>

<!-- Ghost — text-only, for row actions -->
<button class="btn-ghost">Editar</button>

<!-- Danger — for destructive actions -->
<button class="btn-danger">Eliminar</button>

<!-- Icon — square, for close/back -->
<button class="btn-icon">✕</button>
```

**Rules:**
- Only ONE primary button per view/section
- Never use primary for destructive actions
- Icon buttons need `aria-label` for accessibility

---

## Pattern 8: Status Badge / Chip

```html
<span class="chip surface-container-high text-sm px-3 py-1 rounded-full">
    @item.Status
</span>

<!-- Colored variant (use semantic tokens) -->
<span class="chip text-sm px-3 py-1 rounded-full"
      style="background: var(--tertiary-fixed); color: var(--on-tertiary-fixed);">
    Oferta especial
</span>
```

**Rules:**
- Default chip = tonal surface
- Colored chips use semantic tokens (tertiary, success, danger)
- Never use `--primary` for chips — it's reserved for CTAs

---

## CSS Class Reference

| Class | Purpose |
|---|---|
| `.page-container` | Main page wrapper |
| `.page-header` | Title + subtitle area |
| `.page-content` | Main content area |
| `.display` | Page title (editorial scale) |
| `.headline` | Section heading |
| `.body-large` | Subtitle / lead text |
| `.body` | Default body text |
| `.label` | Form labels, table headers |
| `.muted` | Secondary text color |
| `.btn-primary` | Primary CTA (gradient) |
| `.btn-secondary` | Secondary action |
| `.btn-ghost` | Text-only action |
| `.btn-danger` | Destructive action |
| `.btn-icon` | Icon-only button |
| `.input` | Form input / select / textarea |
| `.surface-container` | Card background |
| `.toast` | Notification popup |
| `.chip` | Status badge |
| `.modal-overlay` | Modal backdrop |
| `.modal-content` | Modal card |
| `.empty-state` | Empty state wrapper |
