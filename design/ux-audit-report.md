# Reporte de Auditoría UX — Pana Business Platform

> **Fecha:** 2026-07-05  
> **Versión del design system:** "The Modern Hearth" v2.0  
> **Alcance:** 11 módulos con UI · 15 dimensiones UX · ~45 vistas  
> **Tipo:** Auditoría estática (código fuente) + preparación para auditoría interactiva  
> **Severidad:** 🔴 Crítico · 🟡 Mejora · 🟢 OK · 💡 Oportunidad

---

## 📊 Resumen Ejecutivo

### Hallazgos por severidad

| Severidad | Conteo | Significado |
|-----------|--------|-------------|
| 🔴 Crítico | 12 | Bloquea uso, rompe flujo, datos incorrectos, a11y rota |
| 🟡 Mejora | 47 | Funciona pero degrada experiencia |
| 🟢 OK | — | Cumple o excede |
| 💡 Oportunidad | 18 | Feature que añadiría valor significativo |

### Top 10 Hallazgos Críticos

| # | Módulo | Hallazgo | Severidad |
|---|--------|----------|-----------|
| 1 | **Global** | No-Line Rule violada sistemáticamente — `divide-y`, `border-t`, `border-b` en TODAS las tablas y paneles | 🔴 |
| 2 | **Global** | Sidebar no tiene glass morphism real — usa fondo sólido en vez de `backdrop-blur` | 🔴 |
| 3 | **Production** | Botón "Cerrar día" no pide confirmación — operación destructiva sin guardrail | 🔴 |
| 4 | **POS** | Product detail modal hace múltiples requests al cargar — posible race condition | 🔴 |
| 5 | **Activity** | Duplicación masiva de sidebar nav — mismo bloque de 40 líneas copiado en 3 archivos | 🔴 |
| 6 | **Global** | Estados de loading inconsistentes — skeleton en Dashboard, nada en Activity/Sales/Reports | 🔴 |
| 7 | **Reports** | Botón "Exportar" disabled sin explicación de cuándo estará disponible | 🔴 |
| 8 | **Auth** | Login page duplica tailwind.config y fonts — no comparte con _Layout | 🔴 |
| 9 | **Sales** | Search de ventas no tiene `hx-get`/`hx-trigger` — es solo un input decorativo | 🔴 |
| 10 | **Recipes** | Detail page usa `border-bottom: 1px solid` explícito — rompe No-Line Rule | 🔴 |

---

## 📋 Matriz de Cobertura (Módulo × Dimensión)

| Módulo | Claridad | IA | Consistencia | Feedback | Errores | Eficiencia | a11y | Responsive | DS Compliance | Contenido | Empty | Loading | Densidad | Discoverability | Cross-module |
|--------|----------|----|--------------|----------|---------|------------|------|------------|---------------|-----------|-------|---------|----------|-----------------|--------------|
| Dashboard | 🟢 | 🟢 | 🟡 | 🟢 | 🟡 | 🟢 | 🟡 | 🟡 | 🟡 | 🟢 | 🟢 | 🟢 | 🟢 | 🟡 | 🟡 |
| POS | 🟢 | 🟢 | 🟡 | 🟡 | 🟡 | 🟢 | 🟡 | 🟡 | 🟡 | 🟢 | 🟢 | 🟡 | 🟢 | 🟢 | 🟢 |
| Activity | 🟡 | 🟡 | 🔴 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🟡 | 🔴 | 🟢 | 🟡 | 🟡 |
| Products | 🟢 | 🟢 | 🟡 | 🟢 | 🟡 | 🟢 | 🟡 | 🟢 | 🔴 | 🟢 | 🟢 | 🟢 | 🟢 | 🟢 | 🟡 |
| Sales | 🟢 | 🟢 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🟡 | 🟡 | 🟢 | 🔴 | 🟢 | 🟡 | 🟡 |
| Production | 🟡 | 🟢 | 🟡 | 🔴 | 🔴 | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🟢 | 🟡 | 🟡 | 🟡 | 💡 |
| Raw Materials | 🟢 | 🟢 | 🟡 | 🟢 | 🟡 | 🟢 | 🟡 | 🟢 | 🔴 | 🟢 | 🟢 | 🟢 | 🟡 | 🟢 | 🟡 |
| Recipes | 🟢 | 🟢 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🔴 | 🟡 | 🟢 | 🟡 | 🟢 | 🟡 | 💡 |
| Reports | 🟡 | 🟢 | 🟡 | 🔴 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🔴 | 🟢 | 🟡 | 🟡 |
| Waste | 🟢 | 🟢 | 🟡 | 🟡 | 🟡 | 🟡 | 🟡 | 🟢 | 🟡 | 🟢 | 🟢 | 🟡 | 🟢 | 🟢 | 💡 |
| Auth/Login | 🟢 | 🟢 | 🔴 | 🟡 | 🟡 | 🟢 | 🟢 | 🟢 | 🟡 | 🟢 | N/A | 🟡 | 🟢 | 🟢 | N/A |

---

# MÓDULO 1: DASHBOARD (`/`)

## 🟢 Lo que funciona bien
- **Claridad de propósito**: El greeting personalizado + fecha en español + "Nueva venta" CTA comunican perfectamente qué es y qué hacer.
- **KPIs con lazy-load**: HTMX con skeleton placeholders + auto-refresh cada 60s está muy bien implementado.
- **KPI Drilldown**: Alpine.js slide-down panel para profundizar en revenue/orders/margin/products es excelente UX.
- **Daily context bar**: Muestra factores que afectan ventas (clima, día, quincena) — valor real para el negocio.
- **Sales trend chart**: Chart.js con gradiente, bien integrado visualmente.
- **Empty states**: LowStock widget maneja bien el caso sin productos.

## 🟡 Mejoras

### DSH-01: Stock threshold hardcodeado
**Severidad:** 🟡 | **Dimensión:** Consistencia  
El widget "LowStock" usa `stock <= 10` hardcodeado. Debería usar las reglas de reorden (`ReorderRules`) que ya existen como API.  
**Fix:** `LowStockWidget` debe consultar `IReorderService.GetSuggestions()` y mostrar productos bajo su punto de reorden individual.

### DSH-02: KPIs no cliqueables sin drilldown abierto
**Severidad:** 🟡 | **Dimensión:** Discoverability  
Las KPI cards no tienen cursor pointer ni indican que son interactivas hasta que el usuario descubre el drilldown.  
**Fix:** Agregar `cursor-pointer` y un tooltip "Click para detalle" o un ícono de lupa sutil en hover.

### DSH-03: Legacy widgets no eliminados
**Severidad:** 🟡 | **Dimensión:** Consistencia  
`SalesChartWidget` y `ProductCountWidget` existen como acciones en el controlador pero no se usan en la vista — código muerto.  
**Fix:** Eliminar acciones y partials legacy, o integrarlos si aportan valor.

### DSH-04: No hay link a "Ver todas las ventas" desde RecentSales
**Severidad:** 💡 | **Dimensión:** Cross-module  
El widget "RecentSales" muestra las últimas 5 ventas pero no tiene un link para ir al módulo Sales completo.  
**Fix:** Agregar `hx-boost` link "Ver todas →" al footer del widget.

### DSH-05: Gráfico de sales trend no tiene selector de período
**Severidad:** 💡 | **Dimensión:** Eficiencia  
Siempre muestra 7 días. No hay opción de 30 días, este mes, etc.  
**Fix:** Agregar tabs de período (7d/30d/este mes) con HTMX swap del gráfico.

---

# MÓDULO 2: POS — PUNTO DE VENTA (`/pos`)

## 🟢 Lo que funciona bien
- **Claridad de propósito**: "Punto de Venta" + producto grid + carrito flotante = clarísimo. Es el módulo más intuitivo.
- **Categorías con Alpine.js**: Tabs dinámicos con filtrado instantáneo. Transiciones suaves con gradient al seleccionar.
- **Carrito lateral**: Diseño de panel flotante con glass morphism (`shadow-modal`), empty state con ícono.
- **Dual order type**: Standard + PreOrder integrado en el mismo flujo. Cambio de tipo de orden bien resuelto con Alpine.js.
- **Integración cross-module**: El botón "Nueva pre-orden" en ScheduledOrders redirige al POS — buen flujo.

## 🟡 Mejoras

### POS-01: Validación de pre-order no bloquea submit sin datos requeridos
**Severidad:** 🟡 | **Dimensión:** Errores  
Si el usuario selecciona "Pre-orden" pero no llena CustomerName o ScheduledDate, ¿el form hace submit igual? El controller debe validar pero el feedback en UI no es claro.  
**Fix:** Agregar validación Alpine.js en el form que muestre errores inline antes de permitir submit. Usar `x-bind:disabled` en el botón de pago.

### POS-02: Product detail modal carga innecesariamente productos sin stock
**Severidad:** 🟡 | **Dimensión:** Feedback  
El modal de detalle de producto se carga para todos los productos, incluso los que tienen stock 0.  
**Fix:** El endpoint `/pos/product/{id}` debería mostrar "Agotado" si stock ≤ 0 y deshabilitar "Agregar al carrito".

### POS-03: No hay indicador de "carrito tiene items sin guardar" al navegar fuera
**Severidad:** 🟡 | **Dimensión:** Errores  
Si el usuario navega a otra página con items en el carrito, los pierde sin advertencia.  
**Fix:** Usar `window.addEventListener('beforeunload', ...)` o interceptar clicks en sidebar con Alpine.js para advertir.

### POS-04: Grid de productos no tiene lazy loading ni paginación
**Severidad:** 💡 | **Dimensión:** Eficiencia  
Si hay 100+ productos, la página carga todos de golpe.  
**Fix:** Implementar infinite scroll con HTMX `hx-trigger="revealed"` o paginación por categoría.

### POS-05: No hay búsqueda de productos en POS
**Severidad:** 💡 | **Dimensión:** Eficiencia  
Solo hay tabs de categoría. Si el negocio tiene 50+ productos, el usuario scrollea mucho.  
**Fix:** Agregar search bar con filtrado Alpine.js (ya se usa Alpine, es trivial).

---

# MÓDULO 3: ACTIVITY (`/activity`)

## 🟢 Lo que funciona bien
- **Tres sub-vistas bien diferenciadas**: BillingQueue (acción), ScheduledOrders (planeación), OrderHistory (análisis).
- **Sidebar navigation**: Patrón de sub-nav lateral con íconos SVG y active state claro.
- **Stats cards**: Cada sub-vista tiene cards de resumen relevantes (total, por estado, valor).
- **Integración con POS**: "Nueva pre-orden" → link directo a `/pos`.

## 🔴 Crítico

### ACT-01: Duplicación masiva del sidebar nav — copiado en 3 archivos
**Severidad:** 🔴 | **Dimensión:** Consistencia  
El bloque de navegación lateral (sidebar interno de Activity) de ~40 líneas está copiado idéntico en `BillingQueue.cshtml`, `OrderHistory.cshtml`, y `ScheduledOrders.cshtml`. Solo cambia el `style` del ítem activo.  
**Fix:** Extraer a un ViewComponent `ActivitySidebar` que reciba el `activeTab` como parámetro. Esto elimina ~80 líneas duplicadas y garantiza consistencia.

### ACT-02: BillingQueue no tiene acción de "Entregar" visible en la tabla
**Severidad:** 🔴 | **Dimensión:** Eficiencia  
La cola de facturación muestra órdenes con estado pero no está claro si hay botones de acción (Confirmar, Iniciar Producción, Marcar Listo, Entregar) accesibles directamente.  
**Fix:** Cada fila debe tener el botón de acción correspondiente al estado actual (state machine de `Sale`).

## 🟡 Mejoras

### ACT-03: Filtros de fecha sin funcionalidad HTMX
**Severidad:** 🟡 | **Dimensión:** Feedback  
Los inputs de fecha en OrderHistory no tienen `hx-get`/`hx-trigger`. Son inputs decorativos sin comportamiento.  
**Fix:** Conectar con HTMX: `hx-get="/activity/order-history?from=...&to=..." hx-trigger="change" hx-target="#table-body"`.

### ACT-04: Search de BillingQueue es solo input — sin HTMX
**Severidad:** 🟡 | **Dimensión:** Feedback  
Similar al caso de Sales: el search input no dispara requests HTMX.  
**Fix:** Agregar `hx-get` y `hx-trigger="keyup changed delay:300ms"`.

### ACT-05: Estados de orden en texto plano — sin color
**Severidad:** 🟡 | **Dimensión:** Densidad  
Los estados de orden (Draft, Confirmed, Delivered) se muestran como texto sin badges de color.  
**Fix:** Usar el mismo patrón de badges que `_Detail.cshtml` de Sales (dot + background tonal).

---

# MÓDULO 4: PRODUCTS (`/products`)

## 🟢 Lo que funciona bien
- **CRUD completo con HTMX**: Search-as-you-type, modal create/edit, soft-delete.
- **Avatar de producto**: Iniciales del nombre en gradiente sutil — buen detalle visual.
- **Empty state**: Mensaje claro con CTA "Crea tu primer producto".
- **Margen calculado**: La columna de margen se calcula en la vista — útil.

## 🔴 Crítico

### PRD-01: Formulario usa `border`, `text-gray-*`, y `focus:border-brand-*` — rompe 3 reglas del design system
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
`_Form.cshtml` tiene `border border-gray-200`, `text-gray-700`, `focus:border-brand-300`, y `focus:ring-brand-500/30`. Esto viola:
1. No-Line Rule (usa border explícito)
2. Hardcoded colors (gray-200, gray-700, brand-300)
3. Uso de `brand-*` en vez de tokens `--primary`
**Fix:** Reemplazar con `style="background: var(--surface-container-high);"` y `onfocus="this.style.background='var(--primary-fixed)'; this.style.boxShadow='0 0 0 2px color-mix(in srgb, var(--primary) 20%, transparent)'"`.

## 🟡 Mejoras

### PRD-02: Categoría del producto se muestra como tag pero no se puede filtrar por categoría
**Severidad:** 🟡 | **Dimensión:** Eficiencia  
La tabla muestra la categoría pero no hay filtro de categorías (a diferencia de POS que sí tiene tabs).  
**Fix:** Agregar chips de filtro por categoría sobre la tabla.

### PRD-03: Columna de "Estado" usa colores inconsistentes
**Severidad:** 🟡 | **Dimensión:** Consistencia  
No vi el badge de estado en el fragmento de Products — verificar que use `--success` para activo y `--meta` para inactivo, consistente con el resto de la app.

---

# MÓDULO 5: SALES (`/sales`)

## 🟢 Lo que funciona bien
- **Stats mini-cards**: Total ventas, completadas, borrador, total facturado — resumen inmediato.
- **Detail view**: Excelente presentación con badges de estado, tabla de items, notas en card amber, y sección de pre-order.
- **Badges de estado**: Dot + color semántico + texto — ✨ el mejor patrón de la app.

## 🔴 Crítico

### SAL-01: Search de ventas es solo CSS — sin HTMX
**Severidad:** 🔴 | **Dimensión:** Feedback  
`<input type="search" placeholder="Buscar ventas...">` no tiene `hx-get`, `hx-trigger`, ni `name="q"`. Es un elemento puramente decorativo que no filtra nada.  
**Fix:** Agregar `hx-get="/sales/table-rows" hx-trigger="keyup changed delay:300ms" hx-target="#table-body-sales" name="q"`.

### SAL-02: Botón "Nueva venta" usa `--success` en vez de `--gradient-primary`
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
El CTA principal del módulo usa verde (`--success`) en vez del gradient signature (`--gradient-primary`). Esto rompe la regla "un solo color de acción".  
**Fix:** Cambiar `style="background: var(--success);"` por `style="background: var(--gradient-primary);"`.

## 🟡 Mejoras

### SAL-03: Columna "Estado" en tabla no usa badges de color
**Severidad:** 🟡 | **Dimensión:** Consistencia  
A diferencia del `_Detail.cshtml`, la tabla principal muestra estados como texto.  
**Fix:** Usar el mismo patrón de badge (dot + background tonal).

### SAL-04: Stats cards no son cliqueables para filtrar
**Severidad:** 💡 | **Dimensión:** Eficiencia  
"Completadas", "Borrador" — sería útil poder clickear la card para filtrar la tabla por ese estado.

---

# MÓDULO 6: PRODUCTION (`/production`)

## 🟢 Lo que funciona bien
- **Concepto capturado**: Inicial/Producción/Devolución — refleja exactamente el flujo real de una panadería.
- **Day-closed state**: Vista clara de "Día cerrado" con ícono, resumen, y tabla de lo registrado.
- **Dynamic row adding**: `addProductionRow()` con JS vanilla — simple y funcional.
- **Disponible = Inicial + Producción - Devolución**: Cálculo correcto en la tabla de cierre.

## 🔴 Crítico

### PRO-01: "Cerrar día" no pide confirmación — operación destructiva sin guardrail
**Severidad:** 🔴 | **Dimensión:** Errores  
El botón "Cerrar día" es un `<button type="submit">` directo sin modal de confirmación. Si el usuario lo presiona por error, se generan inventory movements irreversibles (el ledger es append-only).  
**Fix:** Cambiar el `<form method="post">` por un botón que abra un modal: "¿Confirmás el cierre del día? Se actualizará el inventario y no se podrá modificar. [Cancelar] [Cerrar día]".

### PRO-02: `addProductionRow()` no tiene límite — duplicados posibles
**Severidad:** 🔴 | **Dimensión:** Errores  
El usuario puede agregar el mismo producto múltiples veces sin advertencia, generando datos de producción inconsistentes.  
**Fix:** Validar en JS que no se duplique `productId` y mostrar feedback visual si ya existe.

## 🟡 Mejoras

### PRO-03: Tab entre campos no funciona en filas dinámicas
**Severidad:** 🟡 | **Dimensión:** Eficiencia  
El baker necesita moverse rápido entre Inicial→Producción→Devolución con Tab. En filas añadidas dinámicamente, el orden de tabindex puede romperse.  
**Fix:** Asignar `tabindex` secuencialmente al agregar filas.

### PRO-04: No hay autocomplete/suggest de productos
**Severidad:** 💡 | **Dimensión:** Eficiencia  
El select de producto es nativo. Con 50+ productos, sería mejor un searchable select (como el `pana-select` de Sales).  
**Fix:** Usar el componente `pana-select` (Alpine.js) que ya existe en el codebase.

### PRO-05: No se muestran pre-órdenes activas en la vista de producción
**Severidad:** 💡 | **Dimensión:** Cross-module  
`current-task.md` dice "Production view shows active pre-orders for planning" pero en la vista actual no se ve esa información.  
**Fix:** Agregar una sección "Pre-órdenes para hoy" arriba de la tabla de captura, mostrando las órdenes programadas para ese día con sus cantidades.

---

# MÓDULO 7: RAW MATERIALS (`/raw-materials`)

## 🟢 Lo que funciona bien
- **CRUD consistente**: Mismo patrón que Products (search HTMX, modal form, delete).
- **Columnas informativas**: Precio compra, presentación, rendimiento, costo/unidad base — bien pensado para el dominio.
- **Empty state**: Consistente con Products.

## 🔴 Crítico

### RAW-01: Formulario usa patrones v1.0 — mismo problema que Products
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
`_Form.cshtml` probablemente usa los mismos `border`, `text-gray-*`, `focus:border-brand-*` que el form de Products (no leí el archivo completo pero por consistencia de código es muy probable).  
**Fix:** Mismo fix que PRD-01.

## 🟡 Mejoras

### RAW-02: Columna "Costo / unidad base" con 4 decimales — ¿necesario?
**Severidad:** 🟡 | **Dimensión:** Densidad  
`$0.0240/u` mostrado con 4 decimales es difícil de leer. Para una panadería, 2 decimales (centavos) suele ser suficiente.  
**Fix:** `$@material.CostPerBaseUnit.ToString("N2")` en vez de `N4`.

### RAW-03: No hay indicador visual de "stock bajo" de materias primas
**Severidad:** 💡 | **Dimensión:** Efficiency  
Sería valioso ver de un vistazo qué materias primas necesitan reorden. El API de `ReorderRules` ya existe.  
**Fix:** Columna "Stock" con badge warn/rojo según punto de reorden.

---

# MÓDULO 8: RECIPES (`/recipes`)

## 🟢 Lo que funciona bien
- **Detail page**: Layout limpio con back link, tabla de ingredientes con badges de categoría, y sección de costos (próxima).
- **Badges de categoría**: `--tertiary-fixed` / `--on-tertiary-fixed` — consistente con el design system.
- **Empty state de ingredientes**: "Esta receta no tiene ingredientes todavía."

## 🔴 Crítico

### REC-01: Detail page usa `border-bottom: 1px solid` explícito
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
`<div class="px-6 py-4" style="border-bottom: 1px solid var(--surface-container-low);">` rompe la No-Line Rule.  
**Fix:** Usar tonal separation: `style="background: var(--surface-container-low);"` para el header en vez de border-bottom.

### REC-02: Tabla de ingredientes usa `divide-y` en tbody
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
`<tbody class="divide-y" style="border-color: var(--surface-container-low);">` — mismo problema que el resto de tablas.  
**Fix:** Eliminar `divide-y` y usar filas alternadas con `--surface` / `--surface-container-low`.

## 🟡 Mejoras

### REC-03: Costos no se muestran en la lista principal
**Severidad:** 💡 | **Dimensión:** Densidad  
La tabla de índice muestra "Costo total" y "Costo / unidad" pero si no hay ingredientes registrados, muestra 0 sin indicar que falta configurar.  
**Fix:** Mostrar "—" o "Sin ingredientes" cuando `Ingredients.Count == 0`.

### REC-04: No hay "Duplicar receta" 
**Severidad:** 💡 | **Dimensión:** Eficiencia  
Para variantes (ej. Concha de Vainilla → Concha de Chocolate), duplicar y modificar ahorraría tiempo.  
**Fix:** Botón "Duplicar" en detail page → `POST /recipes/{id}/duplicate`.

---

# MÓDULO 9: REPORTS (`/reports`)

## 🟢 Lo que funciona bien
- **Selector de período**: Tabs Hoy/Ayer/Últimos 7 días/Este mes con gradient active state.
- **Metric cards con growth %**: Flechas de tendencia + porcentaje — información accionable.
- **Tablas de detalle**: Daily trend + top products + order history en la misma página.

## 🔴 Crítico

### RPT-01: Botón "Exportar" disabled sin explicación ni fecha
**Severidad:** 🔴 | **Dimensión:** Feedback  
`<button ... disabled title="Exportar — próximamente">` — el usuario no sabe si "próximamente" es mañana o en 6 meses. Esto genera frustración.  
**Fix:** Si el feature no está listo, ocultar el botón hasta que lo esté. O implementarlo (el API `/api/export/sales` ya existe).

### RPT-02: La página completa se recarga al cambiar período — no usa HTMX
**Severidad:** 🔴 | **Dimensión:** Feedback  
Los tabs de período son `<a href="?period=today">` que causan full page reload. Se pierde el estado de scroll y hay flash blanco.  
**Fix:** Usar `hx-get="?period=today" hx-target="#report-content" hx-push-url="true"` para SPA-like navigation.

## 🟡 Mejoras

### RPT-03: "Exportar" en OrderHistory también disabled pero con estilo inconsistente
**Severidad:** 🟡 | **Dimensión:** Consistencia  
El botón "Exportar" en OrderHistory tiene `style="background: var(--surface-container-high); color: var(--fg-2);"` mientras que en Reports es `style="color: var(--fg-2); background: var(--surface-container-low);"`. Inconsistencia visual.  
**Fix:** Usar un ViewComponent `ExportButton` que unifique el disabled state.

---

# MÓDULO 10: WASTE (`/waste`)

## 🟢 Lo que funciona bien
- **Category cards grid**: Visualización clara de tipos de desperdicio con íconos y subcategorías en chips.
- **Botón CTA con `--danger`**: "Registrar desperdicio" usa rojo — semántica correcta para una acción de registro de pérdida.
- **Empty states**: Categorías y registros recientes tienen mensajes claros.

## 🟡 Mejoras

### WST-01: Categorías de desperdicio no son editables desde UI
**Severidad:** 🟡 | **Dimensión:** Eficiencia  
Las categorías vienen precargadas (Quemado, Deformado, Sobrante, etc.) pero no hay UI para agregar/editar/eliminar categorías. El API existe (`/api/waste/categories`).  
**Fix:** Agregar botón "Gestionar categorías" que abra un modal con la lista editable.

### WST-02: No se muestra costo de desperdicio
**Severidad:** 💡 | **Dimensión:** Densidad  
`production-module.md` define `costoMerma = Σ (mermaPorProducto × costoPorUnidad)` pero esto no se muestra en la UI de waste.  
**Fix:** Agregar una card "Costo total de merma del período" en la parte superior.

### WST-03: Integración con Production
**Severidad:** 💡 | **Dimensión:** Cross-module  
Waste debería poder registrar contra un producto específico y un día de producción. Actualmente parece ser genérico.  
**Fix:** Agregar campo "Producto" y "Fecha de producción" al form de registro.

---

# MÓDULO 11: AUTH / LOGIN (`/auth/login`)

## 🟢 Lo que funciona bien
- **Visual impactante**: Animated floating blobs, textura radial, animaciones CSS — excelente primera impresión.
- **Idioma español**: "Iniciar sesión", placeholder "correo@ejemplo.com".
- **Layout standalone**: `Layout = null` correcto para página de auth.

## 🔴 Crítico

### AUTH-01: Login duplica tailwind.config, fonts, y tokens — sin compartir con _Layout
**Severidad:** 🔴 | **Dimensión:** Consistencia  
`Login.cshtml` tiene su propio `<script>tailwind.config=...</script>`, su propio `<link href="fonts.googleapis.com">`, y su propio `<link rel="stylesheet" href="/css/pana-tokens.css">`. Cualquier cambio en el design system requiere actualizar DOS archivos.  
**Fix:** Crear un `_LayoutAuth.cshtml` o usar ViewImports/ViewStart compartido que incluya los assets comunes.

## 🟡 Mejoras

### AUTH-02: No hay "Recordarme" checkbox
**Severidad:** 🟡 | **Dimensión:** Eficiencia  
En un entorno de panadería donde se usa la misma computadora, "Recordarme" reduciría fricción de login diario.  
**Fix:** Agregar checkbox "Recordarme" y manejar con cookie persistente en el backend.

### AUTH-03: Mensaje de error de login es genérico — ¿se muestra inline o con toast?
**Severidad:** 🟡 | **Dimensión:** Errores  
No pude ver la implementación de error de login en el fragmento leído. Si no se muestra inline (sobre el form), agregar.  
**Fix:** Mostrar errores de validación con un alert box sobre el form usando `--danger` background tonal.

---

# HALLAZGOS CROSS-CUTTING

## 🌐 Global — Shell & Navegación

### GLB-01: No-Line Rule violada en TODAS las tablas
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
Virtualmente cada tabla en la app usa `class="divide-y" style="border-color: var(--surface-container-low);"`. Esto viola la No-Line Rule (§2 de DESIGN.md). La separación debe ser por capas tonales alternadas (`--surface` ↔ `--surface-container-low`), no por bordes.  
**Fix:** Plan integral: reemplazar `divide-y` + `border-color` por `even:bg-[var(--surface-container-low)]` en filas. Requiere cambiar TODAS las tablas (10+ archivos). Es la violación más sistemática del design system.

### GLB-02: Sidebar no tiene glass morphism real
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
El sidebar usa `background: var(--surface-container-lowest)` (blanco sólido #ffffff) con `shadow-ambient`. El design system (§4) especifica: "Fondo: `--surface-container-low` al 90% con `backdrop-blur: 16px`. Borde derecho: ghost border `--outline-variant` al 10%."  
**Fix:** Cambiar a `background: rgba(240, 236, 230, 0.90); backdrop-filter: blur(16px); -webkit-backdrop-filter: blur(16px); border-right: 1px solid rgba(228, 189, 186, 0.10);`.

### GLB-03: Sidebar items no usan tokens de sidebar
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
Los items del sidebar principal usan estilos inline inconsistentes en vez de los tokens `--sidebar-bg`, `--sidebar-fg`, `--sidebar-fg-active`, `--sidebar-accent` que YA ESTÁN DEFINIDOS en `pana-tokens.css`.  
**Fix:** Usar `style="color: var(--sidebar-fg);"` para items inactivos y `style="background: var(--sidebar-accent); color: var(--sidebar-fg-active);"` para el activo.

### GLB-04: No hay breadcrumbs
**Severidad:** 🟡 | **Dimensión:** IA  
Navegación profunda (ej. Recipes → Recipe Detail) solo tiene un "← Volver a recetas" manual. No hay breadcrumbs automáticos.  
**Fix:** Implementar un ViewComponent `Breadcrumb` que use la ruta actual para generar el path. Ej: `Productos > Concha de Vainilla`.

### GLB-05: No hay indicador de "página actual" en sidebar principal
**Severidad:** 🟡 | **Dimensión:** IA  
El sidebar principal no resalta el módulo activo. El usuario no sabe en qué sección está sin mirar el título de la página.  
**Fix:** Determinar el módulo activo desde `ViewContext.RouteData` y aplicar `--sidebar-accent` al ítem correspondiente.

### GLB-06: Skeleton loading solo en Dashboard
**Severidad:** 🔴 | **Dimensión:** Loading  
Solo el Dashboard tiene skeleton placeholders. Activity, Sales, Products, Recipes, Raw Materials, Reports, Waste — ninguno muestra loading state durante requests HTMX.  
**Fix:** Agregar `hx-indicator` con animación de skeleton/spinner en todas las tablas que cargan vía HTMX.

### GLB-07: Toast container existe pero ¿se usa?
**Severidad:** 🟡 | **Dimensión:** Feedback  
`<div id="toast-container" class="fixed bottom-4 right-4 z-50 space-y-2"></div>` está en `_Layout.cshtml`. Pero no vi respuestas del servidor que devuelvan toasts (solo redirects y swaps de tabla).  
**Fix:** Agregar toast de éxito después de crear/editar/eliminar: el servidor devuelve el partial de toast con `HX-Trigger: toastEvent` y un script Alpine.js lo inserta en `#toast-container`.

### GLB-08: `pana-select` component está definido en _Layout pero solo se usa en Sales form
**Severidad:** 🟡 | **Dimensión:** Consistencia  
El componente Alpine.js `panaSelect` y sus estilos CSS (~150 líneas) están definidos globalmente en `_Layout.cshtml`, pero solo se usa en `Sales/_Form.cshtml`. Debería usarse en TODOS los forms con selects (Products, Recipes, RawMaterials, Production, Waste).  
**Fix:** Reemplazar `<select>` nativos por `<div x-data="panaSelect">` en todos los formularios.

---

## 🎨 Design System Compliance Global

### DSC-01: Uso masivo de `divide-y` + `border-color` — violación sistemática de No-Line Rule
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
**Archivos afectados:** Products/Index, Sales/Index, Sales/_Detail, Recipes/Index, Recipes/Detail, RawMaterials/Index, Waste/Index, Activity/BillingQueue, Activity/OrderHistory, Activity/ScheduledOrders, Production/Index.  
**Fix:** Plan de remediación:
1. Crear helper CSS `.table-row-even { background: var(--surface-container-low); }` en `pana-tokens.css`
2. En cada `<tr>`: agregar `class="even:bg-[var(--surface-container-low)]"`
3. Eliminar `class="divide-y" style="border-color: ..."` de todos los `<tbody>`

### DSC-02: `--gradient-primary` vs `--success` vs `--danger` — confusión de colores de acción
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
DESIGN.md §4: "`--primary` como ÚNICO color de acción. Solo CTAs críticos." Sin embargo:
- Sales "Nueva venta" usa `--success` (verde)
- Waste "Registrar desperdicio" usa `--danger` (rojo)
- Production "Cerrar día" usa `--success` (verde)
- Solo Products, Recipes, Raw Materials usan `--gradient-primary`
**Fix:** Todos los CTAs primarios deben usar `--gradient-primary`. `--success` y `--danger` son para indicadores semánticos (badges, KPIs), no para botones de acción.

### DSC-03: Hardcoded `border` y `text-gray-*` en forms de Products
**Severidad:** 🔴 | **Dimensión:** DS Compliance  
Ver PRD-01. El form de productos usa clases de Tailwind legacy (`border-gray-200`, `text-gray-700`, `focus:ring-brand-500/30`) que no están en el design system v2.0.  
**Fix:** Reemplazar con tokens: `style="background: var(--surface-container-high); color: var(--fg);"` + onfocus handlers.

### DSC-04: `rounded-2xl` vs `rounded-xl` — inconsistencia de border-radius
**Severidad:** 🟡 | **Dimensión:** DS Compliance  
Algunos contenedores usan `rounded-2xl` (16px) y otros `rounded-xl` (12px) para el mismo propósito. El design system especifica `--radius-lg` (16px) para cards principales.  
**Fix:** Estandarizar: tablas/cards principales → `rounded-2xl`, modales → `rounded-2xl`, botones → `rounded-xl`, inputs → `rounded-xl`.

### DSC-05: `shadow-ambient` se usa como clase de Tailwind pero es un CSS custom property
**Severidad:** 🟡 | **Dimensión:** DS Compliance  
`class="shadow-ambient"` está definido en `tailwind.config` como `'ambient': '0px 4px 16px rgba(28,28,25,0.04), ...'` — esto es correcto. Pero algunos elementos usan `style="box-shadow: var(--elev-ambient);"` y otros `class="shadow-ambient"`. Inconsistencia.  
**Fix:** Estandarizar en `class="shadow-ambient"` ya que Tailwind lo resuelve a las mismas sombras.

---

## ♿ Accesibilidad (a11y)

### ALY-01: No hay focus styles visibles en elementos interactivos
**Severidad:** 🟡 | **Dimensión:** a11y  
Los inputs usan `onfocus`/`onblur` handlers inline para focus visual, pero los botones, links, y selects no tienen focus ring consistente.  
**Fix:** Agregar `focus-visible:ring-2 focus-visible:ring-[var(--primary)] focus-visible:ring-offset-2` en elementos interactivos.

### ALY-02: Íconos SVG no tienen `aria-hidden` ni `aria-label`
**Severidad:** 🟡 | **Dimensión:** a11y  
Todos los SVG inline (search, plus, trash, etc.) no tienen atributos de accesibilidad. Screen readers anunciarán "image" o el contenido del path.  
**Fix:** Agregar `aria-hidden="true"` a SVGs decorativos y `aria-label` a SVGs interactivos.

### ALY-03: Tablas no tienen `<caption>` ni `role` attributes
**Severidad:** 🟡 | **Dimensión:** a11y  
Screen readers necesitan contexto de tabla. Las tablas de datos deben tener al menos `role="table"` y los headers `role="columnheader"`.  
**Fix:** Agregar `scope="col"` a los `<th>` y un `<caption class="sr-only">` descriptivo.

---

## 📱 Responsive / Mobile

### RSP-01: Sidebar no colapsa en mobile — no hay hamburger menu
**Severidad:** 🟡 | **Dimensión:** Responsive  
DESIGN.md §8: "Sidebar: `translate-x-full` en mobile, toggle con botón hamburguesa." No vi implementación de esto en `_Layout.cshtml`.  
**Fix:** Agregar Alpine.js toggle con `x-data="{ sidebarOpen: false }"` y botón hamburguesa en mobile.

### RSP-02: Tablas sin `overflow-x-auto` en algunos casos
**Severidad:** 🟡 | **Dimensión:** Responsive  
Algunas tablas tienen `<div class="overflow-x-auto">` wrapper (Products, Sales) y otras no — inconsistencia.  
**Fix:** Wrap ALL `<table>` en `<div class="overflow-x-auto">` o usar un ViewComponent `DataTable`.

---

## 🔗 API-Only Modules — Gaps

| Módulo API | Ruta | ¿Necesita UI? | Recomendación |
|------------|------|---------------|---------------|
| **Accounting** | `/api/accounting` | 🟡 Sí — journal entries, trial balance | Módulo MVC "Contabilidad" con vista de libro diario y balance |
| **Analytics** | `/api/analytics` | 🟢 No — alimenta Reports | Los datos ya se consumen en `/reports` |
| **Inventory** | `/api/inventory` | 🟡 Sí — stock levels, movements | Módulo MVC "Inventario" con vista de movimientos y niveles |
| **Locations** | `/api/locations` | 💡 Opcional | CRUD simple de sucursales/almacenes |
| **Reorder Rules** | `/api/reorder-rules` | 🟡 Sí — suggestions | Widget "Reorden sugerido" en Dashboard o sección en RawMaterials |

---

# 🗺️ Roadmap de Mejoras

## Semana 1 — Quick Wins (🟡 mejoras de < 1 hora cada una)

| # | Hallazgo | Archivo(s) | Estimado |
|---|----------|------------|----------|
| QW1 | Sales search sin HTMX → agregar `hx-get` | `Sales/Index.cshtml` | 5 min |
| QW2 | Sales CTA usa `--success` → cambiar a `--gradient-primary` | `Sales/Index.cshtml` | 2 min |
| QW3 | Reports tabs → HTMX boost en vez de full reload | `Reports/Index.cshtml` | 15 min |
| QW4 | Activity sidebar duplicado → ViewComponent | `Activity/*.cshtml` + nuevo `ViewComponent` | 30 min |
| QW5 | Activity date filters sin HTMX → agregar `hx-get` | `Activity/OrderHistory.cshtml` | 10 min |
| QW6 | Activity search sin HTMX → agregar `hx-get` | `Activity/BillingQueue.cshtml` | 5 min |
| QW7 | Production "Cerrar día" → agregar modal confirmación | `Production/Index.cshtml` | 20 min |
| QW8 | Dashboard legacy widgets → eliminar código muerto | `DashboardController.cs` + partials | 15 min |
| QW9 | Recetas sin ingredientes → mostrar "—" en vez de 0 | `Recipes/Index.cshtml` | 5 min |
| QW10 | Auth Login → extraer assets a `_LayoutAuth.cshtml` | `Auth/Login.cshtml` + nuevo layout | 30 min |

## Semana 2-3 — Mejoras Medias

| # | Hallazgo | Archivo(s) |
|---|----------|------------|
| MM1 | No-Line Rule: reemplazar `divide-y` por `even:` backgrounds en TODAS las tablas | 10+ archivos .cshtml |
| MM2 | Sidebar glass morphism real | `_Layout.cshtml` |
| MM3 | Sidebar items → usar tokens `--sidebar-*` | `_Layout.cshtml` |
| MM4 | Forms Products/RawMaterials → eliminar hardcoded borders/colors | `Products/_Form.cshtml`, `RawMaterials/_Form.cshtml` |
| MM5 | `pana-select` → usar en todos los forms con selects | 5+ archivos de forms |
| MM6 | Skeleton loading → agregar `hx-indicator` en todas las tablas HTMX | 8+ archivos |
| MM7 | Estados de badge → unificar patrón en BillingQueue y Sales Index | `Activity/BillingQueue.cshtml`, `Sales/Index.cshtml` |
| MM8 | Toast feedback → implementar post-create/edit/delete | Controller actions + partial |
| MM9 | CTA colors → estandarizar `--gradient-primary` | `Sales/Index.cshtml`, `Production/Index.cshtml`, `Waste/Index.cshtml` |
| MM10 | Sidebar colapsable mobile + hamburger menu | `_Layout.cshtml` |

## Mes 1-2 — Proyectos

| # | Hallazgo | Descripción |
|---|----------|-------------|
| PRJ1 | Módulo Accounting UI | Vista MVC de libro diario, trial balance, journal entries |
| PRJ2 | Módulo Inventory UI | Vista MVC de stock levels, movements, stock-in/out |
| PRJ3 | POS: infinite scroll + búsqueda | Lazy loading de productos + search bar Alpine.js |
| PRJ4 | Reports: gráficos (Chart.js) | Barras de ventas, pie de waste, línea de tendencias |
| PRJ5 | Breadcrumbs automáticos | ViewComponent con route-based breadcrumbs |
| PRJ6 | Dark mode / tema | Preparar tokens para tema oscuro (ya hay estructura en Open Design) |
| PRJ7 | Integración Waste ↔ Production ↔ Inventory | Cierre de día → registra waste → actualiza inventory automáticamente |
| PRJ8 | Reorder Rules UI | Vista de reglas de reorden + sugerencias de compra |

---

# 📝 Notas para Auditoría Interactiva (Fase 2)

Cuando se ejecute la app, verificar específicamente:

1. **HTMX responses reales**: ¿Los swaps son instantáneos? ¿Hay flashes de contenido?
2. **Alpine.js state**: ¿`posCart` sobrevive a navegación? ¿`panaSelect` dropdown se posiciona correctamente?
3. **Edge cases**: Formulario vacío → submit, doble click en botones, refresh durante operación, retroceso del navegador
4. **Responsive real**: 1920px, 1280px, 768px, 390px — ¿sidebar colapsa? ¿tablas scrollean?
5. **Teclado**: Tab por el POS, Tab por form de producto, Escape en modales, Enter en search
6. **Errores reales**: Desconectar DB, recargar durante POST, timeout de HTMX

---

> **Próximo paso:** Ejecutar `dotnet run --project src/Pana.Api` y comenzar la auditoría interactiva módulo por módulo.
