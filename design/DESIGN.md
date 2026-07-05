# Pana Design System

> **Category:** Business Operations / SMB Platform
> **Mood:** Professional warmth — panadería moderna con data limpia.
> **Status:** v1.0 · Adoptado 2026-07-04
> **Source of truth:** `src/Pana.Api/wwwroot/css/pana-tokens.css`

Sistema de diseño para Pana Business Platform, siguiendo el schema de 9
secciones de [Open Design](https://github.com/nexu-io/open-design)
(`nexu-io/open-design`). Cada decisión visual está vinculada a un token
en `pana-tokens.css`.

---

## 1. Tema Visual y Atmósfera

- **Mood:** Professional warmth — profesional pero acogedor, como una
  panadería moderna.
- **Feel:** Clean data surfaces with warm undertones. Suficientemente
  serio para operaciones de negocio, suficientemente cálido para una
  panadería familiar.
- **References:** Linear (precisión), Notion (limpieza), panaderías
  artesanales modernas (calidez en los tonos crema).

---

## 2. Paleta de Color y Roles

| Token | Valor | Rol |
|---|---|---|
| `--bg` | `#f8fafc` (slate-50) | Lienzo de página — fondos de dashboard |
| `--surface` | `#ffffff` | Tarjetas, inputs, dropdowns |
| `--surface-warm` | `#faf7f2` | Tarjetas destacadas con tono panadería |
| `--fg` | `#0f172a` (slate-900) | Texto principal |
| `--fg-2` | `#334155` (slate-700) | Texto secundario, labels |
| `--muted` | `#64748b` (slate-500) | Placeholders, help text |
| `--meta` | `#94a3b8` (slate-400) | Timestamps, disabled |
| `--border` | `#e2e8f0` (slate-200) | Bordes de tarjetas, anillos de input |
| `--border-soft` | `#f1f5f9` (slate-100) | Divisores sutiles, filas de tabla |
| `--accent` | `#6366f1` (indigo-500) | **Único color de marca** — CTAs, activo |
| `--accent-hover` | `#4f46e5` (indigo-600) | Hover de botones |
| `--accent-active` | `#4338ca` (indigo-700) | Pressed de botones |
| `--success` | `#16a34a` | KPIs positivos, ventas confirmadas |
| `--warn` | `#f59e0b` | Stock bajo, acciones pendientes |
| `--danger` | `#dc2626` | Eliminaciones, alertas críticas |

### Colores de extensión (B-slot)

| Token | Valor | Rol |
|---|---|---|
| `--warm-accent` | `#d97706` (amber-600) | Toque panadería: tarjetas de producción, mermas |
| `--fresh-green` | `#22c55e` | Indicadores de producto fresco |

### Reglas

- **Un solo color cromático:** `--accent` (indigo). Todo lo demás es
  escala de grises.
- `--warm-accent` es el único color secundario permitido — solo para
  módulos de producción/panadería.
- No introducir un tercer color de acento sin actualizar este documento.

---

## 3. Tipografía

| Token | Valor |
|---|---|
| `--font-display` | `"Inter", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif` |
| `--font-body` | `"Inter", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif` |
| `--font-mono` | `ui-monospace, "SF Mono", "Cascadia Code", "Consolas", monospace` |

### Escala tipográfica

| Token | Size | Uso |
|---|---|---|
| `--text-xs` | 12px | Badges, labels, micro-copy |
| `--text-sm` | 14px | Celdas de tabla, helper text |
| `--text-base` | 16px | Cuerpo, botones |
| `--text-lg` | 18px | Párrafos lead, números KPIs |
| `--text-xl` | 20px | Títulos de tarjeta |
| `--text-2xl` | 24px | Títulos de página |
| `--text-3xl` | 32px | Hero headings |
| `--text-4xl` | 40px | Marketing, landing |

- **Pesos:** 400 (body), 500 (emphasis), 600 (headings), 700 (hero), 800 (display).
- **Inter** es la única familia. Cargar desde Google Fonts con pesos 400;500;600;700;800.

---

## 4. Componentes

### Botones
- **Primario:** `bg-[var(--accent)] text-[var(--accent-on)]` — radius 6px,
  shadow-soft, hover:accent-hover, active:accent-active.
- **Secundario:** `bg-white border border-[var(--border)]` — mismo radius.
- **Ghost:** Sin fondo, solo texto accent — para acciones terciarias.

### Tarjetas (Cards)
- **Fondo:** `--surface` (blanco).
- **Sombra:** `--elev-raised` (doble capa sutil).
- **Radius:** `--radius-md` (8px).
- **Padding interno:** `--space-6` (24px).
- Sin bordes — la separación viene de la sombra.

### Tablas
- **Header:** `text-xs font-semibold text-[var(--muted)] uppercase tracking-wider`.
- **Filas:** `border-b border-[var(--border-soft)]`.
- **Hover:** `bg-[var(--border-soft)]` (slate-50).
- **Padding celda:** `px-4 py-3`.

### Inputs
- **Fondo:** `--surface`.
- **Borde:** `--border`, focus → `--accent` con ring de 2px.
- **Radius:** `--radius-sm` (6px).
- **Placeholder:** `--muted`.

### Sidebar
- **Fondo:** `--sidebar-bg` (indigo-950, gradiente).
- **Texto:** `--sidebar-fg` (indigo-200).
- **Ítem activo:** `bg-[var(--sidebar-accent)] text-white`.
- **Ancho:** 256px (w-64).

---

## 5. Layout

- **Container máximo:** 1280px (`--container-max`).
- **Gutters:** 24px desktop, 16px tablet, 12px phone.
- **Sidebar:** Fijo izquierdo, 256px, colapsable en mobile.
- **Top bar:** Sticky, glass morphism (`bg-white/70 backdrop-blur-xl`).
- **Grid principal:** `grid-cols-1 lg:grid-cols-3 gap-5` para dashboards.
- **Espaciado entre secciones:** `--section-y-*` (64/48/32px).

---

## 6. Profundidad y Elevación

- **Flat:** `none` — fondo de página.
- **Raised:** `--elev-raised` — tarjetas, dropdowns (doble capa sutil).
- **Modal:** `--elev-modal` — overlay + ring para contraste.
- **Sin bordes en tarjetas:** La sombra, no el borde, separa las superficies.
- **Sidebar:** Gradiente sutil (`from-brand-950 via-brand-900 to-brand-950`)
  para dar profundidad sin sombra.

---

## 7. Do's y Don'ts

### ✅ Do
- Usar `var(--accent)` como ÚNICO color de acción.
- Mantener los datos en `--surface` blanco sobre `--bg` slate.
- Usar `--surface-warm` (crema) para tarjetas del módulo de producción.
- Un solo `--accent` por vista — no mezclar indigo con amber en la misma pantalla.
- Preferir sombras sobre bordes para separación de tarjetas.

### ❌ Don't
- No usar `--warm-accent` (amber) como CTA — es solo para contexto de panadería.
- No introducir gradients con colores fuera de la paleta.
- No usar texto claro sobre fondo claro — chequear contraste WCAG AA (4.5:1).
- No hardcodear valores hexadecimales en las vistas Razor — usar clases de Tailwind
  que referencien los tokens.
- No agregar un tercer color de acento sin actualizar este DESIGN.md.

---

## 8. Responsive Behavior

- **Mobile-first:** todas las vistas funcionan en 390px+.
- **Sidebar:** `translate-x-full` en mobile, toggle con botón hamburguesa.
- **Tablas:** Scroll horizontal en pantallas < 768px.
- **Grid:** `grid-cols-1` → `lg:grid-cols-3` para dashboards.
- **Tipografía:** Sin cambios de escala entre breakpoints (se mantiene legible).
- **Gutters:** Se ajustan vía `--container-gutter-*` tokens.

---

## 9. Guía para Agentes

Al generar nuevas vistas Razor (`.cshtml`) para Pana:

1. **Leer `pana-tokens.css` primero** — todos los valores visuales vienen de ahí.
2. **Usar clases de Tailwind** — no escribir CSS arbitrario.
3. **Referenciar tokens** en `tailwind.config` → `colors: { accent: 'var(--accent)' }`.
4. **Seguir la estructura de componentes** definida en §4.
5. **No hardcodear colores** — si necesitas un nuevo color, agregarlo al
   `DESIGN.md` y a `pana-tokens.css` primero.
6. **Idioma:** Español (es-MX). Usar `dddd, d 'de' MMMM 'de' yyyy` para fechas.
7. **HTMX:** Para interacciones AJAX, usar `hx-get`/`hx-post` con partials.
8. **Alpine.js:** Para estado local del UI (toasts, modales, sidebar toggle).

### Stack técnica
- **Backend:** ASP.NET Core 9, Razor Views (.cshtml)
- **CSS:** Tailwind CSS v4 (CDN en dev, PostCSS en prod planeado)
- **Design tokens:** CSS custom properties (`pana-tokens.css`)
- **Interactividad:** HTMX 2.0 + Alpine.js 3.x
- **Gráficos:** Chart.js 4.x
- **Fuente:** Inter (Google Fonts)
