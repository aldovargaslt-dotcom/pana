---
name: one-shot-design
description: 'Design polished, production-ready UI in a single pass using HTMX + Tailwind CSS + modern CSS. Use when: designing new pages, building UI components, creating Razor .cshtml views, adding HTMX interactivity, styling with Tailwind, or polishing existing UI. Covers dashboards, data tables, modals, forms, cards, and full page layouts.'
argument-hint: 'Describe the page or component to design'
user-invocable: true
---

# One-Shot Design — HTMX + Tailwind + CSS

Produce complete, polished Razor `.cshtml` views in a single pass. No iteration needed — every output is ready to paste into the Pana project and run.

## When to Use

- User asks to design or build a new page, view, or UI component
- User mentions "create a page for...", "design the UI for...", "build a view..."
- User wants HTMX-powered interactions (search, modals, inline edit, delete, polling)
- User needs Tailwind-styled components (cards, tables, forms, dashboards)
- User says "polish this", "make it look good", "one-shot design"

## Project Context

Pana is a .NET 8 bakery management system. Key facts:

| Concern | Detail |
|---------|--------|
| **CSS** | Tailwind CSS v4 via CDN (`<script src="https://cdn.tailwindcss.com">`) — no build step |
| **Interactivity** | HTMX 2.0.4 via CDN (`unpkg.com/htmx.org@2.0.4`) — server returns HTML fragments |
| **Client state** | Alpine.js 3.14.9 via CDN — sidebar toggle, toasts, modal open/close |
| **Charts** | Chart.js 4.4.8 via CDN — dashboard charts |
| **Views** | Razor `.cshtml` with `_Layout.cshtml` (fixed sidebar + sticky glass top bar) |
| **Components** | 8 View Components in `Web/Components/` — `Modal`, `DataTable`, `StatCard`, `Button`, `Card`, `EmptyState`, `Tabs`, `NavLink` |
| **Toasts** | Server sets `X-Toast-Message` response header; client JS shows toast |
| **No build tools** | Everything is CDN-based; no npm, no webpack, no wwwroot |

### Layout Structure

```
┌─────────────────────────────────────────────────┐
│ Sidebar (fixed, w-64, brand-950 bg)             │
│  • Brand logo + name                            │
│  • NavLink components                           │
│  • User footer                                  │
├────────────────────┬────────────────────────────┤
│ Top Bar (sticky,   │ @RenderBody()              │
│ glass-morphism,    │ (fade-in animation)        │
│ backdrop-blur)     │                            │
│                    │ #modal-container           │
│                    │ #toast-container (fixed)   │
└────────────────────┴────────────────────────────┘
```

## Design System

Pana's visual language — use these conventions as defaults, but elevate with best-in-class patterns from the references below.

### Colors

Custom `brand` palette (indigo-based, 50–950). Defined in `_Layout.cshtml`'s Tailwind config.

```html
<!-- Primary actions, links, active states -->
bg-brand-600 hover:bg-brand-700 text-brand-600
<!-- Subtle backgrounds -->
bg-brand-50 border-brand-200
<!-- Dark sidebar -->
bg-brand-950
```

### Typography

Font: **Inter** from Google Fonts, set as default `sans`.

```
font-sans antialiased
text-sm text-base text-lg text-xl text-2xl text-3xl text-4xl
font-normal font-medium font-semibold font-bold
tracking-tight tracking-normal tracking-wide
leading-relaxed leading-normal leading-tight
```

### Shadows & Effects

```
shadow-soft    — subtle card elevation
shadow-glow    — branded glow for primary CTAs
shadow-card    — standard card shadow
backdrop-blur-xl bg-white/80  — glass morphism
```

### Animations

Custom CSS classes defined in `_Layout.cshtml`:

```
.fade-in         — opacity 0→1 with translateY(-4px→0)
.slide-in-right  — translateX(16px→0) with opacity
.scale-in        — scale(0.95→1) with opacity
.skeleton        — pulse animation for loading states
.text-gradient   — gradient text with bg-clip-text
```

### View Components (prefer these over raw HTML)

| Component | Use for | Key Props |
|-----------|---------|-----------|
| `Modal` | Forms, confirmations, detail views | `Id`, `Title`, `HxGet`/`HxPost` |
| `DataTable` | Searchable, sortable tables | `HxGet`, `Columns`, `Rows` |
| `StatCard` | KPI metric cards (auto-refresh) | `Title`, `Value`, `HxGet`, `ChangePct` |
| `Button` | Styled buttons with HTMX support | `Text`, `Variant`, `HxGet`/`HxPost` |
| `Card` | Content containers with HTMX support | `HxGet`, `HxTrigger` |
| `EmptyState` | Empty table/list illustrations | `Message`, `Action` |
| `Tabs` | Tabbed navigation (HTMX-loaded panes) | `Tabs`, `ActiveTab` |
| `NavLink` | Sidebar navigation with active state | `Href`, `Icon`, `Text` |

Load the full component API: [View Components Reference](./references/view-components.md)

## One-Shot Design Procedure

Follow these steps in order. Each step builds on the last. Do not skip.

### Step 1: Clarify Scope

Determine what to build. Ask yourself:
- **Full page** or **component/partial**?
- Which module? (Products, Sales, Production, Inventory, Accounting, Analytics, Dashboard)
- What CRUD operations? (list, create, read, update, delete)
- Any special interactions? (search-as-you-type, inline edit, polling, drag-and-drop)

### Step 2: Identify the Data Model

Map the entities and fields from the domain. For Pana modules, consult:
- `design/production-module.md` — RawMaterial, Recipe, RecipeIngredient, production tracking, waste, daily context, BCG matrix
- Existing controllers in `src/Pana.Api/Api/Controllers/` for entity shapes

### Step 3: Choose the Layout Pattern

| Pattern | When to use | HTMX strategy |
|---------|-------------|---------------|
| **Full page with sidebar** | New module page | `_Layout.cshtml` inherited; content in `@RenderBody()` |
| **Data table + search** | List views (products, sales, inventory) | `hx-get` on search input, `hx-target="#table-body"` |
| **Modal form** | Create/edit without navigation | `hx-get` loads form into `#modal-container`, `hx-post` submits |
| **Inline edit** | Quick field edits | `hx-get` replaces row with form, `hx-put` saves |
| **Tabbed view** | Grouped content (details, history, analytics) | `hx-get` loads tab panes on click |
| **Dashboard grid** | KPI overview | `StatCard` components with `hx-trigger="every 60s"` |
| **Card grid** | Browse/gallery (recipes, products) | `hx-get` for filtering, cards with hover effects |

### Step 4: Build the Semantic HTML Skeleton

Write plain, semantic HTML first — no Tailwind classes yet. Focus on:
- Proper heading hierarchy (`h1` → `h2` → `h3`)
- Landmark roles: `<main>`, `<nav>`, `<section>`, `<aside>`
- Form semantics: `<label>`, `<fieldset>`, `<legend>`, proper `name` attributes
- Table semantics: `<thead>`, `<tbody>`, `<th scope="col/row">`
- ARIA labels on interactive elements without visible text

### Step 5: Apply Tailwind Styling — Layer by Layer

Apply classes in this exact order. Each layer is a separate pass:

1. **Layout**: `flex`, `grid`, `grid-cols-*`, `gap-*`, `container`, `mx-auto`, `max-w-*`
2. **Spacing**: `p-*`, `m-*`, `space-y-*`, `space-x-*`
3. **Typography**: `text-*`, `font-*`, `tracking-*`, `leading-*`
4. **Colors**: `bg-*`, `text-*`, `border-*` — use brand palette
5. **Borders & Shadows**: `rounded-*`, `border`, `shadow-*`, `ring-*`
6. **States**: `hover:`, `focus:`, `focus-visible:`, `active:`, `disabled:`
7. **Responsive**: `sm:`, `md:`, `lg:` — mobile-first (unprefixed = mobile)
8. **Dark mode**: `dark:` variants on backgrounds, text, borders

Reference: [Tailwind Cheatsheet](./references/tailwind-cheatsheet.md)

### Step 6: Wire HTMX Interactions

Layer in interactivity. Reference: [HTMX Cheatsheet](./references/htmx-cheatsheet.md)

**Navigation & links**:
```html
<a hx-get="/products" hx-target="#main-content" hx-push-url="true">Products</a>
```

**Search-as-you-type**:
```html
<input hx-get="/products/search" hx-trigger="keyup changed delay:300ms"
       hx-target="#table-body" hx-indicator="#spinner" name="q" />
```

**Modal forms**:
```html
<button hx-get="/products/create-form" hx-target="#modal-container">New</button>
<!-- Form inside modal submits via hx-post, swaps table body, closes modal -->
```

**Inline delete**:
```html
<button hx-delete="/products/@id" hx-confirm="Delete this item?"
        hx-target="#row-@id" hx-swap="delete swap:0.3s">Delete</button>
```

**Polling / auto-refresh**:
```html
<div hx-get="/stats" hx-trigger="every 60s" hx-swap="innerHTML">...</div>
```

**Toast feedback**: Always wire `X-Toast-Message` header on the server side. The client listener in `_Layout.cshtml` handles display automatically.

**Loading indicators**: Every HTMX request should have a visible loading state:
```html
<img class="htmx-indicator" src="/spinner.svg" />  <!-- auto-shown during requests -->
<!-- or -->
<button hx-post="..." hx-indicator="#spinner">Save</button>
```

### Step 7: Add CSS Polish

After structure and interactions work, add the "wow" factor:

- **Card hover**: `group` + `group-hover:shadow-glow transition-shadow duration-300`
- **Button press**: `active:scale-[0.98] transition-transform`
- **Row highlight on new**: `htmx-added` class triggers animation
- **Skeleton loading**: `animate-pulse bg-gray-200 rounded` on placeholder elements
- **Glass panels**: `bg-white/80 backdrop-blur-xl border border-white/20`
- **Gradient accents**: `bg-gradient-to-r from-brand-600 to-brand-400`
- **Staggered list animation**: Use `htmx-added` with `animation-delay` via inline style
- **Smooth swap transitions**: Keep element `id` stable across HTMX responses

### Step 8: Accessibility Check

- [ ] All form inputs have associated `<label>` elements
- [ ] Focus order is logical (tab through the page)
- [ ] `focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500` on all interactive elements
- [ ] `motion-reduce:transition-none` on animated elements
- [ ] Color contrast meets WCAG AA (Tailwind defaults do)
- [ ] `sr-only` text for icon-only buttons/links
- [ ] `aria-label` or `aria-labelledby` on modals and dialogs
- [ ] Table headers use `scope="col"` or `scope="row"`

### Step 9: Output the Complete File

For **full pages**:
```html
@* Views/ModuleName/Index.cshtml *@
@model ModuleNameViewModel
@{
    ViewData["Title"] = "Page Title";
}

<div class="fade-in space-y-6">
    <!-- Page header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">...</h1>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">...</p>
        </div>
        <!-- Actions -->
    </div>
    <!-- Content -->
</div>
```

For **partials/components**:
```html
@* Views/ModuleName/_ComponentName.cshtml *@
@model ComponentViewModel
<!-- Self-contained partial, loaded via HTMX -->
<div id="component-@Model.Id" class="...">
    ...
</div>
```

---

## Quality Checklist

Before outputting the final `.cshtml`, verify every item:

- [ ] **Semantic HTML**: Uses `<main>`, `<nav>`, `<section>`, `<form>`, `<table>` appropriately
- [ ] **State coverage**: All interactive elements have `hover:`, `focus-visible:`, `active:`, `disabled:` variants
- [ ] **Responsive**: Layout works at mobile (default), `sm:`, `md:`, and `lg:` breakpoints
- [ ] **Dark mode**: `dark:` variants on all backgrounds, text, and borders
- [ ] **HTMX indicators**: Every `hx-get`/`hx-post` has a loading indicator
- [ ] **Confirmation**: Destructive actions use `hx-confirm`
- [ ] **Validation**: Forms have `required`, `type`, and `invalid:` styling
- [ ] **Empty state**: Handled with `EmptyState` component or conditional "No items" message
- [ ] **Error state**: Network/server errors surface via toast or inline message
- [ ] **Toast feedback**: Create/update/delete operations set `X-Toast-Message` header (noted for server-side)
- [ ] **Stable IDs**: Elements updated via HTMX have stable `id` attributes for CSS transitions
- [ ] **No inline styles**: All styling via Tailwind utility classes (or View Components)
- [ ] **View Components**: Uses `Modal`, `DataTable`, `StatCard`, `Card`, `Button`, `EmptyState`, `Tabs` where applicable
- [ ] **Layout conventions**: Full pages use `fade-in` wrapper; content respects sidebar + top bar layout
- [ ] **Accessibility**: Labels, focus order, `sr-only`, motion-reduce, ARIA on modals

## Progressive Loading

For detailed syntax reference, the agent loads these files only when needed:

- [HTMX Cheatsheet](./references/htmx-cheatsheet.md) — all attributes, triggers, swap strategies, events, headers
- [Tailwind Cheatsheet](./references/tailwind-cheatsheet.md) — layout, spacing, typography, colors, states, responsive
- [View Components Reference](./references/view-components.md) — full API for all 8 Pana components

## Common Pitfalls

- **Don't use `sm:` for mobile styles.** Unprefixed utilities ARE the mobile styles. `sm:` means "640px and up."
- **Don't forget `id` attributes.** HTMX targets by CSS selector. If an element is updated, give it an `id`.
- **Don't mix `hx-target` levels.** If a form posts and updates a table, the form's `hx-target` must point to the table body, not the modal.
- **Don't skip indicators.** Every HTMX request needs visible feedback. Use `htmx-indicator` class or `hx-indicator` attribute.
- **Don't use inline styles.** Tailwind has a utility for everything. Use arbitrary values like `w-[350px]` for one-offs.
- **Don't nest modals.** Pana's modal system uses a single `#modal-container`. Loading a new modal replaces the old one.
- **Don't forget the toast header.** Server must set `X-Toast-Message` for user feedback after mutations. The client-side listener handles display — you just need to mention it in code comments.
