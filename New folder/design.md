# Business Platform – Architecture & Product Design Request

## Background

I want to build a software platform that initially solves the needs of my family-owned bakery, but I do **not** want to build a "bakery management system."

Instead, I want to build a **Business Platform** (or Business Operating System) that can later support other SMBs such as:

* Bakeries
* Coffee shops
* Restaurants
* Convenience stores
* Small manufacturers
* Retail stores

The bakery is simply the first implementation of the platform.

---

# Vision

The platform should become the operational backbone of a small business.

Everything that happens inside the company should eventually be represented digitally.

Examples:

* Products
* Sales
* Inventory
* Production
* Purchases
* Customers
* Employees
* Reports
* Business documents
* Business knowledge

The platform should centralize operational data while remaining simple enough that a single engineer can build and maintain it.

---

# Long-Term Goal

I want to prepare the platform for AI without overengineering it.

My long-term vision includes:

* AI copilots
* Business insights
* Natural language querying
* Workflow automation
* Planning assistance
* Future AI agents

However, I **do not** want to build an "AI-first architecture" that adds unnecessary complexity today.

Instead, I want the architecture to naturally evolve into that future.

---

# Core Philosophy

I want this project to follow the mindset of a pragmatic senior/staff engineer.

The architecture should prioritize:

* Simplicity
* Maintainability
* Readability
* Low operational cost
* Easy evolution

I explicitly want to avoid "resume-driven development."

---

# Engineering Principles

The design should strongly follow:

* KISS
* YAGNI
* DRY
* SOLID (where appropriate)
* Convention over Configuration
* API First
* Data First
* Optimize for Change, not Scale

I do **not** want complexity unless there is a proven need.

---

# Important Constraint

Think like a lazy senior engineer.

If something can be solved with:

* fewer components
* fewer servers
* fewer technologies
* less operational burden

then prefer that solution.

Avoid introducing technology simply because it is modern.

Every additional component must justify its existence.

---

# Current Thoughts

Instead of microservices, I currently believe a **Modular Monolith** is the correct architecture.

The platform should consist of independent modules inside a single application.

For example:

* Sales
* Inventory
* Production
* Purchasing
* CRM
* Identity
* Reporting
* AI

Modules are created **on demand**, not upfront. The initial bakery implementation likely only needs Products, Sales, and Identity. Other modules (Inventory, Production, CRM, etc.) are added when a real business need arises.

Each module owns its business logic.

The modules communicate through application contracts and domain events rather than direct coupling.

---

# Infrastructure Goals

I want to keep infrastructure extremely simple.

Current idea:

* Ubuntu VPS
* Docker Compose
* Reverse Proxy
* ASP.NET Core application
* PostgreSQL

That's it.

I do **not** currently believe I need:

* Kubernetes
* Kafka
* RabbitMQ
* Redis
* Grafana
* Prometheus
* n8n (or any external workflow engine)
* Service Mesh
* Multiple databases
* Distributed architecture

Unless there is a real justification.

---

# Multi-Tenancy

The platform is designed to support multiple businesses from a single instance.

Every table that holds business data must include a `tenant_id` column from day one.
All queries must be scoped to the current tenant automatically (e.g., via a global query filter in Entity Framework).

This costs ~1 hour to set up today and avoids weeks of refactoring when a second business joins the platform.

---

# Data Philosophy

I believe the data is the most valuable asset.

Applications can be rewritten.

Frameworks change.

Programming languages evolve.

Business data remains.

The platform should therefore prioritize:

* clean data
* normalized data
* documented data
* stable schemas
* auditability

---

# Operational Essentials

Even for a one-engineer platform, some things are non-negotiable:

* **Automated PostgreSQL backups** — pg_dump via cron or Docker. The data is the business.
* **Structured logging** — Serilog (or equivalent). Makes debugging feasible when things break at 2 AM.
* **Health checks** — ASP.NET Core health check endpoints. Tells you if the system is alive without logging into the server.

---

# API Philosophy

Everything should happen through the application.

Nothing should communicate directly with PostgreSQL except the application itself.

Future integrations should consume APIs rather than database tables.

All APIs must expose OpenAPI/Swagger specifications from day one. This serves as living documentation and enables automatic client generation for future consumers.

Examples:

* Website
* Mobile app
* AI
* Automation
* Dashboards

---

# AI Philosophy

I want AI to become a first-class consumer of the platform.

However, I don't want to create unnecessary AI infrastructure today.

My current thought is:

AI

↓

Application Services

↓

Domain

↓

Database

Rather than creating separate AI services immediately.

I also want every business capability to eventually become callable as a tool (potentially MCP-compatible in the future), instead of exposing raw database access.

---

# Event Philosophy

I like the idea of Domain Events.

However, I don't think I need Kafka or RabbitMQ.

I'm leaning toward:

Application Service

↓

Domain Event

↓

Channel<T> (in-process queue)

↓

IHostedService (background worker)

↓

Handlers

Everything inside the same application. No external message broker.

If one day I truly need distributed messaging, I should be able to replace the transport layer (Channel<T> → Kafka/RabbitMQ) without redesigning the domain.

---

# Reporting

I want reporting to be simple.

Current idea:

PostgreSQL

↓

Views (materialized where needed)

↓

Application endpoints (JSON / CSV / Excel export)

No external analytics platform until it becomes necessary. When needed, any BI tool can connect to the API or directly to read-replica views.

---

# Documentation Goals

I want documentation with the quality of a real software architecture project.

Not just README files.

I want documents similar to what a professional engineering team would maintain.

For a one-engineer project, documentation must be lean and maintainable or it will rot.

Essential documents:

* Architecture Decisions (ADR) — why key choices were made
* API Specification — auto-generated via OpenAPI/Swagger
* Database Design — schema diagrams and conventions
* Deployment Guide — how to get the app running from scratch

The documentation should serve as the single source of truth for both the developer and AI coding assistants. If a document isn't updated in 6 months, delete it — stale docs are worse than no docs.

---

# Development Workflow — Agents, MCPs & Skills

How a solo engineer builds this platform with AI assistance, without drowning in tooling.

---

## The Three Layers

```
┌─────────────────────────────────────────┐
│      INSTRUCTIONS (cómo se comporta)     │
│   AGENTS.md / copilot-instructions.md    │
├─────────────────────────────────────────┤
│      MCPs (con qué se conecta)           │
│   Git, PostgreSQL, Filesystem            │
├─────────────────────────────────────────┤
│      AGENTS (quién ejecuta)              │
│   Explore + future specialized agents    │
└─────────────────────────────────────────┘
```

---

## Agents

Start with the minimum. Add only when the pain justifies it.

| Agent | Status | Purpose |
|-------|--------|---------|
| **Explore** | ✅ Built-in | Explore codebase, search files, understand structure |
| **module-scaffolder** | 🔮 Future | Generate new module with consistent folder structure, interfaces, and DI registration |
| **db-migration** | 🔮 Future | Generate EF Core migrations, validate against conventions |
| **code-review** | 🔮 Future | Review PRs against project conventions (Ponytail ladder, SOLID, etc.) |

A single engineer does not need 5 agents on day one. The built-in Explore agent + good instructions covers 90% of the work. Create specialized agents only when you notice yourself repeating the same prompts over and over.

---

## MCPs — Context Providers

| MCP | Status | Value |
|-----|--------|-------|
| **GitKraken** | ✅ Available | Git operations, PRs, issues, commit graph |
| **Filesystem** | ✅ Built-in | Read/write project files |
| **PostgreSQL** | ✅ Recommended: DBHub | Inspect schema, test queries, validate migrations directly from the editor. |
| **Docker** | 🟡 Optional | Container management when deployment grows |

### PostgreSQL MCP — Recommendation

After evaluating the landscape, the top options are:

| Option | Stars | Approach | Verdict |
|--------|-------|----------|---------|
| [DBHub](https://github.com/bytebase/dbhub) | 3.1k | Zero-dependency, 2 tools (`execute_sql`, `search_objects`), TOML config | ✅ **Pick this** |
| [Official MCP PG](https://github.com/modelcontextprotocol/servers-archived/tree/main/src/postgres) | 88k (monorepo) | Read-only, schema inspection, archived but stable | 🟡 Fallback |
| [pg-aiguide](https://github.com/timescale/pg-aiguide) | 1.8k | Postgres skills + docs for AI, not a DB connector | 🔮 Future complement |

DBHub wins because: zero dependencies, token-efficient (only 2 MCP tools = less context window waste), multi-database support, built-in web workbench, and explicit VS Code/Copilot support.

**Setup (add to VS Code MCP config):**
```json
{
  "mcpServers": {
    "dbhub": {
      "command": "npx",
      "args": ["-y", "@bytebase/dbhub@latest", "--dsn", "postgres://user:password@localhost:5432/pana"]
    }
  }
}
```

On Windows, wrap with `cmd /c`:
```json
{
  "mcpServers": {
    "dbhub": {
      "command": "cmd",
      "args": ["/c", "npx", "-y", "@bytebase/dbhub@latest", "--dsn", "postgres://user:password@localhost:5432/pana"]
    }
  }
}
```

The PostgreSQL MCP is the biggest multiplier: ask "how is the products table structured?" or "generate a migration that adds a low_stock_threshold column" without leaving the editor.

---

## Skills & Instructions — The Brain

### The Ponytail Ladder

After evaluating [Ponytail](https://github.com/DietrichGebert/ponytail) (73k stars, measured -54% LOC, -20% cost, 100% safety retention), it aligns perfectly with this project's philosophy. However, the full Ponytail plugin ecosystem (lifecycle hooks, MCP, slash commands, statuslines, 10+ adapter formats) is itself a violation of its own principle.

**Decision: Use Ponytail Lite approach — a single instruction file, no plugins.**

The YAGNI ladder every AI agent must climb before writing code:

```
1. Does this need to exist?         → No: skip it (YAGNI)
2. Already in this codebase?        → Reuse it
3. .NET stdlib / framework does it? → Use it
4. Native platform feature?         → Use it (e.g., <input type="date">)
5. Installed dependency?            → Use it
6. One line?                        → One line
7. Only then: the minimum that works
```

Lazy about the solution, never about reading the code first. Trust-boundary validation, data-loss handling, security, and accessibility are **never** on the chopping block.

### Project instructions files

| File | Purpose |
|------|---------|
| `.github/copilot-instructions.md` | Rules loaded by GitHub Copilot in VS Code |
| `AGENTS.md` | Rules loaded by Cursor, Codex, Windsurf, and others |

Both files contain the same Ponytail-inspired ladder + project conventions.

---

# What I Want From You

I would like you to act as a pragmatic Staff Software Engineer.

Challenge my assumptions.

Point out where I am overengineering.

Point out where I am underengineering.

Recommend simpler alternatives whenever possible.

Help me design a platform that can realistically be maintained by one engineer for years while still leaving room for future growth.

I am **not** looking for the most modern architecture.

I am looking for the architecture that provides the best balance between:

* simplicity
* maintainability
* extensibility
* AI readiness
* operational cost

Most importantly:

**Do not optimize for hypothetical scale. Optimize for long-term maintainability.**
