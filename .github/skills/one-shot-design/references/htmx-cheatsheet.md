# HTMX Cheatsheet

Quick reference for HTMX 2.x attributes and patterns. For full docs, see [htmx.org/docs](https://htmx.org/docs/).

## Core Request Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `hx-get` | GET request | `hx-get="/products/search"` |
| `hx-post` | POST request (create) | `hx-post="/products"` |
| `hx-put` | PUT request (full update) | `hx-put="/products/1"` |
| `hx-patch` | PATCH request (partial update) | `hx-patch="/products/1"` |
| `hx-delete` | DELETE request | `hx-delete="/products/1"` |

## Trigger Patterns (`hx-trigger`)

| Pattern | Use for |
|---------|---------|
| `click` (default) | Buttons, links — no attribute needed |
| `submit` (default) | Forms — no attribute needed |
| `keyup changed delay:300ms` | Search-as-you-type |
| `keyup changed delay:500ms` | Slower search (more server-friendly) |
| `every 2s` | Polling at interval |
| `every 60s` | Dashboard stat refresh |
| `load` | Fire once on element load |
| `load delay:1s` | Delayed load (progress bars, deferred content) |
| `revealed` | Lazy load when element enters viewport |
| `intersect threshold:0.5` | Fire when 50% visible |
| `mouseenter once` | Fire once on hover |
| `custom-event from:body` | Listen for custom events from body |
| `product-created from:body` | Refresh after another element creates data |

### Trigger Modifiers

```
changed          — only fire if value changed
once             — fire at most once
delay:<time>     — debounce (resets timer on each trigger)
throttle:<time>  — throttle (drops events during cooldown)
from:<selector>  — listen on different element
target:<selector> — listen on different element's events
```

## Target Patterns (`hx-target`)

| Value | Meaning |
|-------|---------|
| `#id` | Specific element by ID |
| `.class` | Element by class (first match) |
| `this` | The element itself |
| `closest tr` | Nearest ancestor `<tr>` |
| `closest form` | Nearest ancestor `<form>` |
| `next .result` | Next sibling matching `.result` |
| `previous .header` | Previous sibling matching `.header` |
| `find input` | First descendant `<input>` |

## Swap Strategies (`hx-swap`)

| Value | Behavior |
|-------|----------|
| `innerHTML` (default) | Replace content inside target |
| `outerHTML` | Replace the target element entirely |
| `beforebegin` | Insert before target (as sibling) |
| `afterbegin` | Prepend inside target |
| `beforeend` | Append inside target |
| `afterend` | Insert after target (as sibling) |
| `delete` | Remove target element |
| `none` | Don't swap (OOB + headers only) |

### Swap Modifiers

```
swap:0.3s        — 300ms delay between clear and insert
settle:0.1s      — 100ms delay before settling
scroll:top       — scroll target to top after swap
scroll:bottom    — scroll target to bottom
show:top         — scroll target's top into view
show:bottom      — scroll target's bottom into view
ignoreTitle:true — don't update document title
transition:true  — use View Transitions API
```

### Swap for Delete (animated removal)

```html
<button hx-delete="/items/1" hx-target="#row-1"
        hx-swap="delete swap:0.3s">Delete</button>
```

## CSS Transitions via HTMX

Keep the `id` attribute stable across responses. When new content has the same `id` as old content, htmx copies old attributes to new elements, then settles new attributes — CSS transitions fire automatically.

```html
<!-- Before response -->
<div id="status" class="bg-gray-100">Pending</div>

<!-- After response (same id, different class) -->
<div id="status" class="bg-green-100">Complete</div>
```

```css
#status {
    transition: all 0.3s ease;
}
```

## Out-of-Band Swaps (`hx-swap-oob`)

Update multiple page regions from a single response:

```html
<!-- Server response -->
<div id="toast" hx-swap-oob="true">Item saved!</div>
<div id="counter" hx-swap-oob="true">5 items</div>
<!-- Main content (swapped into hx-target) -->
<tr>...</tr>
```

For `<tr>`, `<td>`, `<th>` (which can't stand alone), wrap in `<template>`:

```html
<template>
  <tr id="row-1" hx-swap-oob="true"><td>...</td></tr>
</template>
```

## Indicators & Loading States

### Auto-indicator (simplest)

```html
<button hx-post="/save">
    Save
    <img class="htmx-indicator" src="/spinner.svg" />
</button>
```

The `.htmx-indicator` class sets `opacity: 0` by default. HTMX adds `.htmx-request` to the triggering element, which makes `.htmx-indicator` visible via `opacity: 1`.

### Explicit indicator

```html
<div>
    <button hx-post="/save" hx-indicator="#spinner">Save</button>
    <div id="spinner" class="htmx-indicator">Saving...</div>
</div>
```

### Disable during request

```html
<button hx-post="/save" hx-disabled-elt="this">Save</button>
```

## Confirmation

```html
<!-- Simple browser confirm -->
<button hx-delete="/items/1" hx-confirm="Delete this item?">Delete</button>

<!-- Inheritance: hoist to parent -->
<div hx-confirm="Are you sure?">
    <button hx-delete="/items/1">Delete</button>
    <button hx-put="/items/1">Update</button>
    <button hx-confirm="unset" hx-get="/">Cancel</button>
</div>
```

## Response Headers (set on server)

| Header | Effect |
|--------|--------|
| `HX-Trigger` | Fire client-side event after swap |
| `HX-Trigger-After-Settle` | Fire event after settle |
| `HX-Trigger-After-Swap` | Fire event after swap |
| `HX-Redirect` | Client-side redirect |
| `HX-Refresh` | Full page refresh |
| `HX-Retarget` | Change swap target (CSS selector) |
| `HX-Reswap` | Override swap strategy |
| `HX-Push-Url` | Push URL to history |
| `X-Toast-Message` | **Pana custom**: show toast notification |

## Events (client-side JS)

| Event | When |
|-------|------|
| `htmx:load` | New content loaded into DOM |
| `htmx:afterSwap` | After content swapped |
| `htmx:afterSettle` | After settle delay |
| `htmx:beforeSwap` | Before swap (can cancel) |
| `htmx:configRequest` | Before request sent (modify params/headers) |
| `htmx:responseError` | Server returned error |
| `htmx:sendError` | Network error |
| `htmx:validation:validate` | Before form validation |
| `htmx:validation:failed` | Validation failed |
| `htmx:confirm` | Before confirm dialog (for custom dialogs) |

Events fire in both camelCase and kebab-case (`htmx:afterSwap` / `htmx:after-swap`).

## HTMX Request Headers (sent automatically)

| Header | Value |
|--------|-------|
| `HX-Request` | `true` (identifies HTMX requests) |
| `HX-Current-URL` | Current browser URL |
| `HX-Target` | ID of target element |
| `HX-Trigger` | ID of triggering element |
| `HX-Trigger-Name` | `name` of triggering element |

**Server pattern**: Check `HX-Request` header to return partial vs full page.

## Inheritance

Most HTMX attributes inherit to children:

```html
<div hx-target="#results" hx-indicator="#spinner">
    <input hx-get="/search" name="q" />  <!-- inherits target + indicator -->
    <button hx-get="/filter" name="cat" /> <!-- inherits target + indicator -->
</div>
```

Use `hx-disinherit="*"` to prevent inheritance on a child.

## Validation

```html
<form hx-post="/items" hx-validate="true">
    <input name="email" type="email" required
           class="invalid:border-red-500 focus:invalid:ring-red-500" />
    <button type="submit">Submit</button>
</form>
```

Set in `_Layout.cshtml` for native browser validation:
```js
htmx.config.reportValidityOfForms = true;
```

## Boosting (SPA-like navigation)

```html
<div hx-boost="true">
    <a href="/products">Products</a>  <!-- becomes AJAX GET -->
    <a href="/sales">Sales</a>        <!-- becomes AJAX GET -->
</div>
```

Degrades gracefully — links work without JavaScript.

## History

```html
<a hx-get="/page" hx-push-url="true">Page</a>
<!-- Back button works; old DOM restored from cache -->
```

Disable history for sensitive pages:
```html
<div hx-history="false">...</div>
```

## Debugging

```js
htmx.logAll();  // Log all HTMX events to console
```
