# Pana — AI Context Pack

> **Paste this into any AI chat (Gemini, ChatGPT, Claude) before discussing Pana.**
> One page. No fluff. Everything the AI needs to give useful answers.

---

## What is Pana?

A **Business Platform** (modular monolith) for SMB operations — starting with a bakery. Think: products, sales, inventory, recipes, production. One app, one database, one developer.

## Tech Stack

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core 9, C# 12 |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core (EnsureCreated, no migrations) |
| Frontend | Razor .cshtml + HTMX + Tailwind CSS |
| Auth | JWT (hardcoded dev secret — prod uses env var) |
| Logging | Serilog |
| Tests | xUnit 2.9 (unit tests only, no integration tests) |
| Deploy | Docker Compose on single VPS |

## Architecture at a Glance

```
Modular Monolith — folders as modules, NOT microservices.

Pana.Api/
├── Api/Controllers/      ← REST JSON API + MVC controllers
├── Application/{Module}/ ← Services, DTOs (C# records), FluentValidation
├── Domain/{Module}/      ← Entities with behavior, value objects, domain events
├── Infrastructure/Data/  ← EF DbContext, EF configs, tenant resolution
├── Web/                  ← ViewModels, ViewComponents, TagHelpers
└── Views/                ← Razor .cshtml pages
```

**No MediatR. No CQRS. No Redis. No Kafka.** Services call services directly.

## Critical Constraints (DO NOT IGNORE)

1. **Every table has `tenant_id`** — EF global query filter enforces isolation. All queries through DbContext.
2. **No `Stock` table** — inventory is a ledger: `SUM(Quantity)` of `InventoryMovement` rows (append-only).
3. **No EF migrations** — app uses `EnsureCreated()`. Never run `dotnet ef migrations add`.
4. **One class per file** — DTOs are `record` types, validators suffix `Validator`, EF configs suffix `Configuration`.
5. **`.snake_case()`** for DB columns, `PascalCase` for C#.
6. **API routes**: `/api/{resource}` (no `/v1/` prefix). `[Authorize]` on every controller.
7. **Hardcoded default tenant**: `00000000-0000-0000-0000-000000000001` — intentional for single-tenant phase.
8. **Domain events are fire-and-forget** — lost on restart. Don't rely on them for data consistency.

## Design System

**"The Modern Hearth"** — warm artisanal Mexican bakery aesthetic:
- Palette: parchment layers (creams #fcf9f4 → #e5e2dd), single accent: oxide red `#9e0012`
- **No 1px borders** — separation by tonal transitions only
- Glassmorphism for nav, 135° gradient for CTAs
- Warm tinted shadows (never pure black)
- Font: Plus Jakarta Sans
- See `design/DESIGN.md` for full spec

## Current Modules

| Module | Status |
|---|---|
| Identity / Auth | ✅ Live |
| Products | ✅ Live |
| Sales | ✅ Live |
| Inventory (ledger) | ✅ Live |
| Accounting | ✅ Live |
| Analytics | ✅ Live |
| Waste Tracking | ✅ Live |
| Production (recipes, costing) | 🟡 In design — see `design/production-module.md` |
| Locations | ✅ Live |
| Export | ✅ Live |

## Key Documentation

| File | Purpose |
|---|---|
| `AGENTS.md` | AI agent instructions (YAGNI ladder, conventions, pitfalls) |
| `design/DESIGN.md` | Full design system spec |
| `design/adr.md` | Architecture Decision Records |
| `design/production-module.md` | Bakery domain knowledge |
| `design/ai-context.md` | This file — cross-AI context pack |
