# Architecture Decision Records (ADR)

## ADR-001: Modular Monolith over Microservices

**Date:** 2026-07-04
**Status:** Accepted

**Context:** The platform needs to support multiple business domains (products, sales, inventory, identity) while being maintainable by a single engineer.

**Decision:** Use a Modular Monolith — a single ASP.NET Core application with modules organized as folders (Domain, Application, Api per module). No separate services, no inter-service communication overhead.

**Consequences:**
- ✅ Simple deployment: one binary, one database
- ✅ Easy refactoring across module boundaries
- ✅ Low operational cost (single VPS)
- ⚠️ Cannot scale modules independently (not needed now)
- ⚠️ Requires discipline to keep modules decoupled via contracts

---

## ADR-002: PostgreSQL as the Single Database

**Date:** 2026-07-04
**Status:** Accepted

**Context:** The platform needs to store relational business data (products, sales, users) with strong consistency.

**Decision:** PostgreSQL only. No Redis, no MongoDB, no read replicas for now.

**Consequences:**
- ✅ One database to backup, monitor, and manage
- ✅ JSONB support for semi-structured data if needed
- ✅ Views for reporting queries
- ✅ Full-text search built in (no need for Elasticsearch)
- ⚠️ May need read replicas if reporting queries become heavy (future concern)

---

## ADR-003: Multi-Tenancy via tenant_id Column

**Date:** 2026-07-04
**Status:** Accepted

**Context:** The platform will eventually serve multiple SMBs from a single instance.

**Decision:** Every business-data table includes a `tenant_id` column. Entity Framework global query filters enforce tenant isolation automatically. A hardcoded default tenant serves the initial single-bakery phase.

**Consequences:**
- ✅ Adding a second tenant requires zero schema changes
- ✅ ~1 hour of upfront cost vs weeks of future refactoring
- ✅ Tenant isolation enforced at the ORM level
- ⚠️ All queries must go through DbContext (no raw connections)

---

## ADR-004: In-Process Domain Events (Channel<T> + IHostedService)

**Date:** 2026-07-04
**Status:** Accepted

**Context:** Modules need to react to events in other modules (e.g., a sale should deduct inventory) without tight coupling.

**Decision:** Use `System.Threading.Channels.Channel<T>` as an in-process message queue with `IHostedService` background workers. No Kafka, no RabbitMQ.

**Consequences:**
- ✅ Zero operational overhead
- ✅ Fire-and-forget within the same process
- ✅ Replaceable transport layer (swap Channel<T> for Kafka later if needed)
- ⚠️ Events are lost on process restart (acceptable for phase 1)

---

## ADR-005: Inventory as a Movement Ledger

**Date:** 2026-07-04
**Status:** Accepted

**Context:** The platform needs to track product stock levels accurately and auditably.

**Decision:** No separate `Stock` table. Stock is computed as the SUM of all `InventoryMovement` records. Each movement records the type (StockIn, StockOut, Adjustment, SaleDeduction).

**Consequences:**
- ✅ Fully auditable — every stock change has a reason and user
- ✅ No stock/count discrepancy — the ledger IS the truth
- ✅ Simpler than maintaining a Stock table + reconciliation logic
- ⚠️ Stock queries aggregate over time; may need materialized views for very large datasets

---

## ADR-006: Ponytail Lite as AI Instruction Set

**Date:** 2026-07-04
**Status:** Accepted

**Context:** AI coding agents tend to over-engineer solutions by default.

**Decision:** Use a single `AGENTS.md` / `.github/copilot-instructions.md` file with the Ponytail YAGNI ladder (7 rungs from "does this need to exist?" to "minimum that works"). No plugins, no MCP servers, no hooks.

**Consequences:**
- ✅ Forces AI to justify every line of code
- ✅ One file to maintain, zero tooling overhead
- ✅ Measured -54% LOC in Ponytail benchmarks
- ⚠️ Requires the engineer to also follow the ladder (lead by example)
