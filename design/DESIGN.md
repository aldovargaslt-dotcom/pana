# Pana Design System — "The Modern Hearth"

> **Category:** Business Operations / SMB Platform — Panadería
> **Creative North Star:** The Modern Hearth — calidez artesanal mexicana con
>   estética editorial premium.
> **Status:** v2.0 · Adoptado 2026-07-04 (reemplaza v1.0 "Professional Warmth")
> **Source of truth:** `src/Pana.Api/wwwroot/css/pana-tokens.css`

Sistema de diseño para Pana Business Platform, siguiendo el schema de 9
secciones de [Open Design](https://github.com/nexu-io/open-design)
(`nexu-io/open-design`). Cada decisión visual está vinculada a un token
en `pana-tokens.css`.

**Creative North Star:** "The Modern Hearth" rechaza la estética estéril
de "app genérica". Traduce la experiencia sensorial de una panadería
mexicana —el calor del horno, la textura artesanal de la harina, la
cultura vibrante del barrio— a una experiencia editorial digital de alto
nivel. Usamos **asimetría intencional** y **profundidad tonal** en lugar
de grillas rígidas.

---

## 1. Tema Visual y Atmósfera

- **Mood:** The Modern Hearth — calidez artesanal mexicana con pulido editorial.
- **Feel:** Capas de pergamino, tonos crema, acentos rojo óxido. Como una
  panadería de barrio retratada por una revista de diseño.
- **References:** Editorial design (asimetría, tipografía display),
  panaderías mexicanas tradicionales (colores tierra, texturas de pan),
  glassmorphism moderno (barras flotantes, profundidad sin bordes).
- **Idioma:** Español (es-MX). Tono cercano pero profesional.

---

## 2. Paleta de Color y Roles

### La regla "No-Line"
**Prohibido usar bordes de 1px sólido para separar contenido.** Los
límites se definen exclusivamente mediante transiciones de color de fondo
o cambios tonales sutiles. Ejemplo: una sección de descripción de producto
no lleva stroke; se coloca sobre `--surface-container-low` (#f6f3ee)
mientras el fondo principal es `--surface` (#fcf9f4).

### Jerarquía de superficies (capas de pergamino)

| Token | Valor | Rol |
|---|---|---|
| `--bg` | `#fcf9f4` | Lienzo principal — Level 0 (base) |
| `--surface` | `#fcf9f4` | Alias — mismo que `--bg` |
| `--surface-container-low` | `#f6f3ee` | Level 1 — bloques de fondo grandes |
| `--surface-container` | `#f0ede9` | Level 2 — tarjetas, elementos interactivos |
| `--surface-container-high` | `#ebe8e3` | Campos de input, áreas de datos |
| `--surface-container-highest` | `#e5e2dd` | Level 3 — contenido elevado crítico |
| `--surface-container-lowest` | `#ffffff` | Tarjetas de producto, overlay blancos |

### Foreground (escala tonal)

| Token | Valor | Rol |
|---|---|---|
| `--fg` | `#1c1c19` | Texto principal, headings |
| `--fg-2` | `#3d3d38` | Texto secundario, labels |
| `--muted` | `#6b6b63` | Placeholders, help text |
| `--meta` | `#94948c` | Timestamps, disabled |

### Acentos

| Token | Valor | Rol |
|---|---|---|
| `--primary` | `#9e0012` | **Único color de marca** — CTAs principales |
| `--primary-container` | `#c41e24` | Gradiente de botón, hover |
| `--primary-fixed` | `#ffdad6` | Fondo de input en focus |
| `--on-primary` | `#ffffff` | Texto sobre `--primary` |
| `--tertiary` | `#654700` | Chips de información, "Freshness Pulse" |
| `--tertiary-fixed` | `#ffdea9` | Fondo de ofertas especiales, badges |
| `--on-tertiary-fixed` | `#271900` | Texto sobre `--tertiary-fixed` |
| `--outline-variant` | `#e4bdba` | "Ghost border" — solo accesibilidad, 15% opacity |

### Semánticos

| Token | Valor | Rol |
|---|---|---|
| `--success` | `#16a34a` | KPIs positivos, ventas confirmadas |
| `--warn` | `#f59e0b` | Stock bajo, acciones pendientes |
| `--danger` | `#dc2626` | Eliminaciones, alertas críticas |

### Reglas de color

- **Un solo color cromático de acción:** `--primary` (rojo óxido). No usar
  otros colores para CTAs.
- **El gradiente de botón** es la "Firma Visual": 135° de `--primary` a
  `--primary-container`. Solo para botones primarios.
- **Tonal layering:** Las superficies se diferencian por tono, no por bordes.
- **Glassmorphism:** `--surface` al 80% + `backdrop-blur: 12px` para barras
  flotantes y checkout.
- **Sombras cálidas:** NUNCA usar negro puro. Las sombras se tiñen con
  `--fg` (#1c1c19) a opacidad <10%.
- `--outline-variant` SOLO para accesibilidad (high-contrast mode). Usar
  al 15% de opacidad. Bordes opacos de alto contraste están prohibidos.

---

## 3. Tipografía

| Token | Valor |
|---|---|
| `--font-display` | `"Plus Jakarta Sans", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif` |
| `--font-body` | `"Plus Jakarta Sans", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif` |
| `--font-mono` | `ui-monospace, "SF Mono", "Cascadia Code", "Consolas", monospace` |

### Escala tipográfica editorial

| Token | Size / Line-height | Uso |
|---|---|---|
| `--text-display-lg` | 48px / 1.1 | Hero statements ("Aroma headlines") |
| `--text-display-md` | 40px / 1.15 | Títulos de landing |
| `--text-display-sm` | 32px / 1.2 | Subtítulos hero |
| `--text-headline-lg` | 28px / 1.25 | "Baker's Signature" — categorías |
| `--text-headline-md` | 24px / 1.3 | Títulos de página |
| `--text-headline-sm` | 20px / 1.35 | Títulos de sección |
| `--text-body-lg` | 18px / 1.6 | Párrafos lead, números KPI |
| `--text-body-md` | 16px / 1.6 | Cuerpo, botones |
| `--text-body-sm` | 14px / 1.6 | Celdas de tabla, helper text |
| `--text-label-md` | 14px / 1.4 | Labels en All-Caps, +0.05em tracking |
| `--text-label-sm` | 12px / 1.4 | Badges, micro-copy, All-Caps |

- **Pesos:** 400 (body), 500 (emphasis), 600 (headings), 700 (display), 800 (hero display).
- **Display:** Negative letter-spacing (-0.02em) para look editorial tight.
- **Labels:** Siempre All-Caps con +0.05em letter-spacing — look arquitectónico limpio.
- **Plus Jakarta Sans** es la única familia. Cargar desde Google Fonts con pesos 400;500;600;700;800.
- **Inter** queda deprecada. No usar en nuevas vistas.

---

## 4. Componentes

### Botones

- **Primario (Signature Texture):** Gradiente 135° `--primary` → `--primary-container`.
  Altura mínima 3.5rem (`spacing-10`). Radius `--radius-md` (12px). Sin borde.
  Hover: intensificar gradiente (shift 5% más claro). Active: escala 0.98.
- **Secundario:** Sin fondo. Texto `--primary`. "Ghost Border": `--outline-variant`
  al 15% de opacidad. Mismo radius.
- **Teriario:** Fondo `--tertiary-fixed` (#ffdea9), texto `--on-tertiary-fixed`
  (#271900). Para "Ofertas especiales" o "Nuevo".

### Tarjetas (Cards)

- **Regla "No-Divider":** Prohibido usar líneas divisorias. Separar items de lista
  con `spacing-4` (1.4rem) de espacio vertical o alternando fondos entre
  `--surface` y `--surface-container-low`.
- **Product Cards:** Fondo `--surface-container-lowest` (#ffffff). Las imágenes
  del producto deben sobrepasar el borde superior de la tarjeta por `spacing-2`
  (0.7rem) para romper la grilla — look editorial premium.
- **Fondo:** `--surface-container` (#f0ede9).
- **Sombra:** `--elev-ambient` (difusa, cálida, sin negro puro).
- **Radius:** `--radius-lg` (16px).
- **Padding:** `--space-6` (24px).

### Tablas

- **Header:** `text-label-sm font-semibold text-[var(--muted)]`.
- **Filas:** Sin bordes. Separación por `spacing-2` (8px) vertical.
  Alternar `--surface` ↔ `--surface-container-low` en filas pares/impares.
- **Hover:** `--surface-container-low` (transición tonal, no color).
- **Padding celda:** `px-4 py-3`.

### Inputs

- **Fondo:** `--surface-container-high` (#ebe8e3).
- **Sin borde inferior.** Solo 4px de border-radius (`--radius-sm`).
- **Focus:** Fondo cambia a `--primary-fixed` (#ffdad6) + "ghost border" de
  2px `--primary` al 20% de opacidad.
- **Placeholder:** `--muted`.

### Sidebar

- **Fondo:** Superficie cálida con glassmorphism — `--surface-container-low`
  al 90% con `backdrop-blur: 16px`.
- **Borde derecho:** "Ghost border" `--outline-variant` al 10%.
- **Texto:** `--fg-2`.
- **Ítem activo:** Fondo `--primary` al 10%, texto `--primary`, sin borde.
- **Ancho:** 256px (w-64).
- **Brand:** "Pana" en `--text-headline-sm` peso 700, color `--primary`.

### Featured Components

- **"The Freshness Pulse":** Chip con `--tertiary` (#654700), animación de pulso
  suave. Indica pan horneado en los últimos 30 minutos.
- **"Asymmetric Hero":** Layout donde la imagen del producto se alinea a la
  derecha sangrando la pantalla, mientras el `display-lg` se superpone al
  borde izquierdo de la imagen.

---

## 5. Layout

- **Container máximo:** 1280px (`--container-max`).
- **Gutters:** 24px desktop, 16px tablet, 12px phone.
- **Sidebar:** Fijo izquierdo, 256px, colapsable en mobile. Glass morphism.
- **Top bar:** Sticky, glass morphism (`--surface` al 80% + `backdrop-blur: 12px`).
- **Grid principal:** `grid-cols-1 lg:grid-cols-3 gap-5` para dashboards.
- **Secciones:** Usar `spacing-12` y `spacing-16` para márgenes de sección —
  permitir que el layout "respire".
- **Asimetría:** Romper la grilla con imágenes que sangran contenedores y
  tipografía que se superpone a imágenes.

---

## 6. Profundidad y Elevación

### Principio de Capas Tonales
La profundidad se logra por capas de color, no por líneas estructurales.
Colocar una tarjeta `--surface-container-lowest` (#ffffff) sobre una
sección `--surface-container-low` (#f6f3ee) crea un "levantamiento"
suave y orgánico.

### Sombras Ambientales
Para elementos flotantes (FAB del carrito, modales):
- **Valor:** `0px 12px 32px rgba(28, 28, 25, 0.06)`
- **Regla:** El color de sombra DEBE ser un tinte de `--fg` (#1c1c19),
  NUNCA negro puro.

| Token | Uso |
|---|---|
| `--elev-flat` | `none` — fondo de página |
| `--elev-ambient` | Tarjetas, dropdowns — sombra difusa cálida |
| `--elev-modal` | Overlay + ring para contraste |

- **Glassmorphism:** `--surface` al 80% opacidad + `backdrop-blur: 12px`
  para navegación flotante.
- **Sidebar:** Glass morphism con blur, sin gradiente.

---

## 7. Do's y Don'ts

### ✅ Do
- Usar `--primary` (rojo óxido) como ÚNICO color de acción. Propósito: solo
  CTAs críticos.
- Usar transiciones tonales (capas de pergamino) en lugar de bordes.
- Usar `spacing-12` y `spacing-16` para que el layout "respire".
- Fotografía grande de alta resolución del pan — las texturas del pan dulce
  son parte del UI.
- Usar sombras cálidas (tinte `--fg`, nunca negro).
- Mínimo `--radius-sm` (4px) en TODO elemento — cero sharp corners.

### ❌ Don't
- **NO usar divisores de 1px ni bordes para separar contenido.**
- **NO usar esquinas sharp.** Todo debe tener mínimo `sm` (4px) de roundedness.
- **NO usar sombras "Material Design Blue" o grises estándar.** Sombras cálidas.
- **NO saturar la pantalla.** Si se siente apretado, aumentar el espaciado
  dos incrementos en la escala.
- **NO hardcodear valores hexadecimales en vistas Razor** — usar tokens CSS.
- **NO usar `--primary` fuera de CTAs.** No pintar headings, íconos, o
  decoraciones con el rojo de marca.
- **NO agregar un tercer color de acento sin actualizar este DESIGN.md.**

---

## 8. Responsive Behavior

- **Mobile-first:** todas las vistas funcionan en 390px+.
- **Sidebar:** `translate-x-full` en mobile, toggle con botón hamburguesa.
- **Tablas:** Scroll horizontal en pantallas < 768px.
- **Grid:** `grid-cols-1` → `lg:grid-cols-3` para dashboards.
- **Tipografía:** Display scale colapsa en mobile (display-lg → display-md).
- **Gutters:** Se ajustan vía `--container-gutter-*` tokens.
- **Glass morphism:** Se desactiva en mobile (perf): fondo sólido en su lugar.

---

## 9. Guía para Agentes

Al generar nuevas vistas Razor (`.cshtml`) para Pana:

1. **Leer `pana-tokens.css` primero** — todos los valores visuales vienen de ahí.
2. **Usar clases de Tailwind** — no escribir CSS arbitrario.
3. **Referenciar tokens** en `tailwind.config` → `colors: { primary: 'var(--primary)' }`.
4. **Seguir la estructura de componentes** definida en §4.
5. **No hardcodear colores** — si necesitas un nuevo color, agregarlo al
   `DESIGN.md` y a `pana-tokens.css` primero.
6. **Idioma:** Español (es-MX). Usar `dddd, d 'de' MMMM 'de' yyyy` para fechas.
7. **HTMX:** Para interacciones AJAX, usar `hx-get`/`hx-post` con partials.
8. **Alpine.js:** Para estado local del UI (toasts, modales, sidebar toggle).
9. **No-Line Rule:** Nunca usar `border`, `border-b`, `divide-y`. Separar con
   espacio (`gap-*`, `space-y-*`) o fondos alternados.
10. **Asimetría:** Romper la grilla — imágenes que sangran contenedores,
    tipografía que se superpone.

### Stack técnica
- **Backend:** ASP.NET Core 9, Razor Views (.cshtml)
- **CSS:** Tailwind CSS v4 (CDN en dev, PostCSS en prod planeado)
- **Design tokens:** CSS custom properties (`pana-tokens.css`)
- **Interactividad:** HTMX 2.0 + Alpine.js 3.x
- **Gráficos:** Chart.js 4.x
- **Fuente:** Plus Jakarta Sans (Google Fonts) — reemplaza Inter
