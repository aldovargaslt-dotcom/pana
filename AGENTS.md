# Pana — AI Agent Instructions

You are working on the **Pana Business Platform**: a modular monolith (ASP.NET Core 9 + PostgreSQL 16) that runs SMB operations (bakeries, inventory, sales). The best code is the code you never wrote.

---

## Build / Test / Run

| Action | Command |
|---|---|
| Restore | `dotnet restore src/Pana.sln` |
| Build | `dotnet build src/Pana.sln` |
| Run (dev) | `dotnet run --project src/Pana.Api` → `http://localhost:5202` |
| Run (Docker) | `docker compose up -d` (PostgreSQL + API) |
| Test | `dotnet test src/Pana.Tests` |
| Publish | `dotnet publish src/Pana.Api -c Release -o ./out` |

- **.NET 9.0**, **C# 12**, **xUnit 2.9** for tests, **Serilog** for logging.
- No linter config, no CI/CD in repo yet.
- The app uses `EnsureCreated()` — **no EF migrations**. Do not run `dotnet ef migrations add`.

---

## The YAGNI Ladder

Before writing ANY code, climb this ladder. Stop at the first rung that holds:

```
1. Does this need to exist?          → No: skip it (YAGNI)
2. Already in this codebase?         → Reuse it, don't rewrite
3. .NET stdlib / framework does it?  → Use it
4. Native platform feature?          → Use it
5. Installed dependency?             → Use it
6. One line?                         → One line
7. Only then: the minimum that works
```

Climb the ladder AFTER understanding the problem — read the files the change touches, trace the real flow, then pick a rung.

---

## Never Cut

These are never on the chopping block:

- Input validation and trust-boundary checks
- Data-loss handling (transactions, rollbacks)
- Authentication and authorization
- Tenant isolation (every query must be tenant-scoped)
- Error handling that prevents silent data corruption

---

## Architecture

**Modular Monolith** — one ASP.NET Core app, modules as folders. See [ADR-001](design/adr.md#adr-001-modular-monolith-over-microservices) for rationale.

```
Pana.Api/
├── Api/Controllers/      ← REST API (JSON) + MVC views
├── Application/{Module}/ ← Services, DTOs (records), FluentValidation validators
├── Domain/{Module}/      ← Entities with behavior, value objects, domain events, interfaces
├── Infrastructure/Data/  ← EF Core DbContext, EF configurations, tenant resolution
├── Web/                  ← MVC frontend (ViewModels, ViewComponents, TagHelpers)
└── Views/                ← Razor .cshtml
```

- **Layers**: Domain → Application → Infrastructure → Api (see [design.md](New%20folder/design.md))
- **No MediatR, no CQRS** — services call services directly.
- **Both REST API and MVC UI** in the same process. API controllers are in `Api/Controllers/`; MVC controllers are in `Web/Controllers/`.
- Modules communicate through contracts (`ISalesService`) and domain events, not direct references.

### Key Architectural Decisions

All ADRs are in [design/adr.md](design/adr.md):
- **ADR-001**: Modular Monolith over Microservices
- **ADR-002**: PostgreSQL as the single database (no Redis, no MongoDB)
- **ADR-003**: Multi-tenancy via `tenant_id` column + EF global query filters
- **ADR-004**: In-process domain events (`Channel<T>` + `IHostedService`)
- **ADR-005**: Inventory as a movement ledger (no `Stock` table)

---

## Conventions

### Multi-Tenancy (CRITICAL)

- **Every business-data table** has a `tenant_id` column. See [ADR-003](design/adr.md#adr-003-multi-tenancy-via-tenant_id-column).
- EF Core **global query filters** enforce tenant isolation automatically.
- **No query may bypass the tenant filter.** Always use `PanaDbContext` — never raw SQL connections.
- The current hardcoded default tenant is `00000000-0000-0000-0000-000000000001`. The `Tenants` table itself is NOT tenant-filtered.

### Naming

- **C#**: PascalCase for public, camelCase for private/parameters.
- **Database**: `snake_case` for tables (`products`, `sales`, `inventory_movements`) and columns (`tenant_id`, `created_at`).
- **One class per file**.
- **DTOs are C# `record` types** (positional records): `ProductDto`, `CreateSaleRequest`.
- **Validators suffix `Validator`**: `ProductRequestValidator`.
- **EF configurations suffix `Configuration`**: `ProductConfiguration`.
- **Service interfaces**: `I{Module}Service` — one interface per module.
- **API routes**: `/api/{resource}` (no version prefix), `[Authorize]` on every controller.

### Domain Entities

Entities are **not anemic** — they encapsulate behavior:
- Private setters, public methods for state changes (`SetName()`, `Activate()`, `Confirm()`).
- Guard clauses in constructors/setters (throw `ArgumentException`).
- **State machines** for entities with lifecycle: `Sale` uses an `AllowedTransitions` dictionary; illegal transitions throw `InvalidOperationException`.
- Every entity inherits from `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`) or `TenantEntity` (adds `TenantId`).
- `MarkUpdated()` is called in every setter/mutator.
- EF Core gets a `private` parameterless constructor.

### Inventory: Ledger Pattern

**No `Stock` table.** Stock is `SUM(Quantity)` of `InventoryMovement` rows:
- Append-only (insert only, never update/delete).
- Movement types: `StockIn`, `StockOut`, `Adjustment`, `SaleDeduction`, `Transfer`, `ProductionIn`, `ProductionOut`.
- Outgoing = negative quantities. See [ADR-005](design/adr.md#adr-005-inventory-as-a-movement-ledger).

### Domain Events

`Channel<T>` + `IHostedService` — in-process, fire-and-forget:
- `DomainEventDispatcher` publishes, `DomainEventBackgroundWorker` dispatches to handlers.
- Handlers implement `IDomainEventHandler<T>`.
- Events are **lost on process restart** (acceptable for phase 1). See [ADR-004](design/adr.md#adr-004-in-process-domain-events-channelt--ihostedservice).

### Validation & Errors

- **FluentValidation** with `AddFluentValidationAutoValidation()` — automatic model validation in controllers.
- Return `ProblemDetails` on errors. Log structured errors via Serilog.
- Never expose stack traces to the client in production.
- Controllers return `ActionResult<T>`.

---

## If Asked to Add a Dependency

Push back. Every new NuGet package must justify:
- What problem does it solve that the framework/stdlib doesn't?
- Can 20 lines of code do the same thing?
- Will this dependency still be maintained in 2 years?

---

## Known Pitfalls

1. **No EF migrations** — the app uses `EnsureCreated()`. Do not add migrations or run `dotnet ef migrations` commands.
2. **No pagination** — `GetAll` endpoints return full lists. If adding new list endpoints, consider pagination.
3. **No API versioning** — routes are flat (`/api/products`), no `/v1/` prefix.
4. **Hardcoded tenant** — the fallback tenant GUID appears in multiple files. Do not remove it — it's intentional for the single-tenant phase.
5. **Domain events are ephemeral** — lost on restart. Don't rely on them for critical data consistency.
6. **No integration tests** — only unit tests exist. Services and controllers aren't tested.
7. **appsettings.json contains JWT secret** — dev only. Production should use env vars (`JWT__KEY`).
8. **MVC + API hybrid** — be careful not to mix MVC and API concerns. API controllers go in `Api/Controllers/`, MVC controllers in `Web/Controllers/`.

---

## Domain Knowledge

- **Production module**: See [design/production-module.md](design/production-module.md) for bakery-specific domain model (raw material costing, recipe costing, unit conversions, waste tracking, daily context, BCG matrix).
- **Platform vision**: See [New folder/design.md](New%20folder/design.md).

---

## Workflow Protocol (CRITICAL)

Every feature implementation MUST follow this protocol. These gates exist to prevent the #1 bug source: shipping code that doesn't match expectations.

### Gate A — Spec Confirmation (BEFORE any code)

After reading a spec or feature request, restate what you understand in **3-5 bullet points** and ask for confirmation:

> "Here's what I understand we're building:
> - [concrete deliverable 1]
> - [concrete deliverable 2]
> - [concrete deliverable 3]
> Confirm this is correct before I start?"

**Do not write a single line of code until the user confirms.** This prevents the "that's not what I asked for" class of bugs.

### Gate B — QA Pre-Ship Checklist (BEFORE every deploy)

Before running `ship.ps1` or any deploy command, complete the checklist in [design/qa-checklist.md](design/qa-checklist.md):

1. Build: `dotnet build src/Pana.sln` — 0 errors
2. Tests: `dotnet test src/Pana.Tests` — all pass
3. Architecture: controllers in right folders, `[Authorize]`, tenant filter
4. UI: HTMX attributes correct, anti-forgery tokens, no hardcoded styles
5. Routes: new endpoints return correct status codes
6. Cleanup: no debug code, no commented-out blocks

**Report results to the user before shipping.** Never skip this gate.

### Gate C — Ship

Only after Gates A and B both pass:

```powershell
.\ship.ps1
```

This runs build → test → deploy in one command. If any step fails, stop and report.

---

## Documentation Policy

- Update [ADRs](design/adr.md) when making architectural decisions.
- The API spec is auto-generated from controllers — keep them clean.
- Don't write documentation a future you won't maintain.
