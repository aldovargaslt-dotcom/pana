# Pana Platform — Handout de Actividades

**Fecha:** Julio 4-5, 2026  
**URL producción:** `http://198.58.111.94`  
**Repositorio:** `github.com/aldovargaslt-dotcom/pana`

---

## 1. ¿Qué hicimos hoy?

### 🐛 Bugs corregidos

| # | Bug | Síntoma | Fix |
|---|---|---|---|
| 1 | **HTMX bloqueado por SRI** | Ningún botón funcionaba, `hx-get` ignorado | Hash `integrity` incorrecto en `<script>` de HTMX. Se removió. |
| 2 | **`EnsureDeletedAsync` en DB gestionada** | Crash al iniciar: `database "postgres" does not exist` | DB gestionadas de Linode no permiten dropear la DB. Se cambió por solo `EnsureCreatedAsync()`. |
| 3 | **`<!option>` con tipos anónimos** | Error 500 en formularios de ventas y waste | Tipos anónimos son `internal` → no accesibles desde el view engine. Se reemplazó por `ExpandoObject`. |
| 4 | **Modales invisibles** | Alpine.js no inicializa contenido insertado por HTMX | Se eliminó Alpine de los modales. Ahora usan `style="position:fixed;display:flex"` inline + `onclick` vanilla JS. |
| 5 | **`RedirectToAction` en POST HTMX** | Formulario causaba redirect en vez de partial update | Se cambió por devolver `PartialView` directamente con `HX-Trigger`. |

### 🚀 Infraestructura

| Acción | Detalle |
|---|---|
| **Base de datos migrada** | De PostgreSQL local en Docker → **Managed Database de Linode** (`a491886-akamai-prod-7170107-default.g2a.akamaidb.net:23741`) |
| **VPS configurado** | Ubuntu 24.04 en Linode (`198.58.111.94`), Docker + Git |
| **CI/CD improvisado** | `git push` → `ssh VPS "git pull && docker compose up -d --build"` |

### 🆕 Módulos nuevos

| Módulo | Archivos | URL |
|---|---|---|
| **Waste Tracking** | `Api/Controllers/WasteController.cs`, `Web/Controllers/WasteController.cs`, `Web/ViewModels/WasteViewModel.cs`, `Views/Waste/Index.cshtml`, `Views/Waste/_Form.cshtml`, `Views/Waste/_TableRows.cshtml` | `/waste` |
| **Production Capture** | `Web/Controllers/ProductionController.cs`, `Views/Production/Index.cshtml` | `/production` |

---

## 2. Estado actual de la plataforma

### Módulos completos (backend + frontend)

| Módulo | Domain | Service | API | Web UI | URL |
|---|---|---|---|---|---|
| **Dashboard** | — | — | — | ✅ KPI cards, widgets, daily context | `/` |
| **Productos** | ✅ | ✅ CRUD | ✅ REST | ✅ Lista, formulario modal, tabla HTMX | `/products` |
| **Ventas** | ✅ State machine | ✅ CRUD + transiciones | ✅ 9 endpoints | ✅ Lista, formulario modal multi-item | `/sales` |
| **Waste Tracking** | ✅ | ✅ Seed + CRUD | ✅ REST | ✅ Lista, formulario modal, tabla HTMX | `/waste` |
| **Production Capture** | ✅ | ✅ Upsert + Close | ✅ REST | ✅ Formulario diario, cerrar día | `/production` |
| **Auth** | ✅ JWT + BCrypt | ✅ Register/Login | ✅ REST | ✅ Login page | `/auth/login` |

### Módulos con backend pero sin UI web

| Módulo | Domain | Service | API | Lo que falta |
|---|---|---|---|---|
| **Inventory** | ✅ Ledger | ✅ Stock in/out/adjust | ✅ REST | Página de niveles de stock, historial de movimientos |
| **Locations** | ✅ Jerárquico | ✅ CRUD | ✅ REST | Página de gestión de ubicaciones |
| **Reorder Rules** | ✅ | ✅ Con sugerencias | ✅ REST | Página de reglas de reorden |
| **Accounting** | ✅ Doble entrada | ✅ CRUD + trial balance | ✅ REST | Página de libro diario |
| **Raw Materials** | ✅ | ✅ CRUD | ✅ REST | Página de materias primas |
| **Recipes** | ✅ Full costing | ✅ CRUD + ingredientes | ✅ REST | Página de recetas |
| **Analytics** | — | ✅ P&L, waste, trends | ✅ REST | Páginas de reportes |
| **Export** | — | ✅ CSV | ✅ REST | Botones de descarga |

### Módulos NO implementados

| Módulo | Estado | Esfuerzo estimado |
|---|---|---|
| **Customers CRM** | Sin empezar | 3-4h — nueva entidad `Customer`, relación con `Sale`, vistas |
| **Suppliers / Purchases** | Sin empezar | 4-6h — `Supplier`, `PurchaseOrder`, `PurchaseOrderLine`, flujo completo |
| **Unit of Measure UI** | Entidad existe, sin service/UI | 1-2h |

---

## 3. ¿Cómo probar cada módulo?

### 🧪 Pruebas manuales

#### Productos
1. Ir a `http://198.58.111.94/products`
2. Click **Nuevo** → llenar nombre, SKU, precio, costo → **Crear Producto**
3. Verificar que aparece en la tabla con margen calculado
4. Click en la fila (editar) o botón eliminar

#### Ventas
1. Ir a `http://198.58.111.94/sales`
2. Click **Nueva venta** → seleccionar producto → cantidad → precio → **Crear Venta**
3. Verificar que aparece en la tabla con estado "Draft"

#### Waste Tracking
1. Ir a `http://198.58.111.94/waste`
2. Click **Registrar desperdicio** → seleccionar producto → cantidad → motivo → **Registrar**
3. Verificar que aparece en la tabla de registros recientes

#### Production Capture
1. Ir a `http://198.58.111.94/production`
2. Click **Agregar producto** → seleccionar → llenar inicial/producción/devolución
3. Click **Guardar producción**
4. Click **Cerrar día** → verifica que queda en modo solo lectura

### 🧪 Pruebas API (curl desde el VPS)

```bash
# Health
curl http://localhost:8080/health

# Crear producto
curl -X POST http://localhost:8080/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Pan Francés","sku":"PAN-001","price":2.50,"cost":0.80}'

# Ver stock
curl http://localhost:8080/api/inventory

# Waste categories
curl http://localhost:8080/api/waste/categories

# P&L report
curl "http://localhost:8080/api/analytics/profit-loss?from=2026-06-01&to=2026-07-04"
```

---

## 4. Arquitectura

```
Pana.Api/
├── Api/Controllers/      ← REST API (JSON) — 12 controllers
├── Application/{Module}/ ← Services, DTOs, Validators
├── Domain/{Module}/      ← Entities con comportamiento
├── Infrastructure/Data/  ← EF Core DbContext + Configurations
├── Web/
│   ├── Controllers/      ← MVC controllers (HTMX) — 6 controllers
│   ├── ViewModels/       ← ViewModels (records)
│   └── Components/       ← ViewComponents + TagHelpers
└── Views/                ← Razor .cshtml — 6 módulos
```

**Stack:** ASP.NET Core 9 · PostgreSQL 16 · EF Core · HTMX 2.0 · Tailwind CSS CDN · Alpine.js 3.14 · Chart.js 4.4

**Patrones clave:**
- **Modular Monolith** — módulos como carpetas, servicios se llaman directo (sin MediatR)
- **Multi-tenant** — `tenant_id` en cada tabla, EF global query filter
- **Inventory Ledger** — sin tabla `Stock`, el stock es `SUM(Quantity)` de `InventoryMovements`
- **Domain Events** — `Channel<T>` + `IHostedService` (in-process, fire-and-forget)
- **State Machine** — `Sale` tiene transiciones permitidas (Draft→Confirmed→...→Completed)

---

## 5. Convenciones

| Categoría | Regla |
|---|---|
| **C#** | PascalCase público, camelCase privado, records para DTOs |
| **DB** | `snake_case`, `tenant_id` en TODA tabla de negocio |
| **Una clase por archivo** | |
| **DTOs** | `record` types (inmutables) |
| **Validators** | Sufijo `Validator`, FluentValidation |
| **EF Configs** | Sufijo `Configuration`, `ValueGeneratedNever()` para GUIDs |
| **API routes** | `/api/{resource}`, `[Authorize]` en todos |
| **Web routes** | `/{module}`, sin prefijo |
| **HTMX partials** | Prefijo `_`, retornados como `PartialView()` |
| **Modales** | Vanilla JS inline styles, NO Alpine `x-show` |

---

## 6. Próximos pasos

### 🔴 Prioridad alta
1. **Probar Waste y Production manualmente** — asegurar que el flujo completo funcione
2. **Agregar seed data de Waste Categories** — llamar `SeedDefaultsAsync()` al iniciar si está vacío
3. **Arreglar docker-compose.prod.yml persistente** — el `git reset --hard` lo revierte cada vez

### 🟡 Prioridad media
4. **Customers CRM** — entidad `Customer` (Name, Phone, Email), relación con `Sale`, vista de clientes
5. **Reports UI** — aprovechar Analytics API existente, crear páginas de P&L y tendencias
6. **Inventory UI** — página de niveles de stock con tabla + filtros

### 🟢 Prioridad baja
7. **Suppliers / Purchases** — dominio completo desde cero
8. **Tailwind CSS build** — migrar de CDN a build local (mejor performance)
9. **HTTPS** — certificado SSL con Let's Encrypt en el VPS

---

## 7. Comandos útiles

```bash
# Desarrollo local (necesita PostgreSQL)
cd src/Pana.Api && dotnet run
# → http://localhost:5202

# Deploy a producción
git push
ssh root@198.58.111.94 'cd /home/pana/app && git pull && cd deploy && docker compose -f docker-compose.prod.yml up -d --build api'

# Ver logs producción
ssh root@198.58.111.94 'docker logs pana-api --tail 50'

# Backup manual
ssh root@198.58.111.94 'docker exec pana-db pg_dump -U pana pana > backup.sql'

# Health check
curl http://198.58.111.94/health
```

---

## 8. Notas para multi-tenant (cafetería futura)

Para agregar un nuevo negocio solo se necesita:

```sql
INSERT INTO tenants (id, name, slug, created_at)
VALUES (gen_random_uuid(), 'Café del Centro', 'cafe-centro', now());
```

El aislamiento de datos es automático por el `tenant_id` en cada tabla y el global query filter de EF Core. Lo que faltaría construir:
- Selector de tenant en el login/header
- Roles y permisos por tenant
- Personalización visual por tenant (logo, colores)
