# QA Pre-Ship Checklist

> **Run before every `docker compose up -d --build api` deployment.**
> Copilot must complete this checklist and report results before shipping.

---

## Gate 1 — Compilation

```powershell
dotnet build src/Pana.sln
```

- [ ] **0 errors, 0 warnings** (or only pre-existing warnings)
- [ ] No missing `using` statements
- [ ] No type mismatch errors
- [ ] All project references resolve

---

## Gate 2 — Unit Tests

```powershell
dotnet test src/Pana.Tests
```

- [ ] **All tests pass** (0 failures)
- [ ] No skipped tests without justification
- [ ] New code has corresponding tests (if applicable)

---

## Gate 3 — Architecture Compliance

- [ ] New API controllers are in `Api/Controllers/` (NOT `Web/Controllers/`)
- [ ] New MVC controllers are in `Web/Controllers/` (NOT `Api/Controllers/`)
- [ ] Every API controller has `[Authorize]` attribute
- [ ] All new DB queries go through `PanaDbContext` (no raw SQL connections)
- [ ] New entities inherit from `BaseEntity` or `TenantEntity`
- [ ] New business-data tables include `tenant_id` column
- [ ] EF configurations use `.snake_case()` for table/column names
- [ ] DTOs are C# `record` types (positional records)
- [ ] Validators suffix `Validator`, EF configs suffix `Configuration`
- [ ] No EF migrations added (`EnsureCreated()` only)

---

## Gate 4 — UI Integrity (when views are changed)

- [ ] New `.cshtml` views reference `_Layout.cshtml` or appropriate layout
- [ ] HTMX attributes are correct:
  - `hx-post` / `hx-get` / `hx-delete` point to valid routes
  - `hx-target` references existing DOM elements
  - `hx-swap` uses appropriate strategy
- [ ] Buttons have correct `type` attribute (submit/button)
- [ ] Forms include anti-forgery tokens (`@Html.AntiForgeryToken()`)
- [ ] CSS classes use `pana-tokens.css` design tokens (not hardcoded colors)
- [ ] No raw `style=""` attributes — use Tailwind or design tokens
- [ ] New pages are accessible via navigation (not orphaned)

---

## Gate 5 — Route Smoke Test

After build succeeds locally, verify new/modified routes:

- [ ] `GET` routes return 200 (not 500, not 404)
- [ ] `POST` routes with valid data return success (200/201/302)
- [ ] `POST` routes with invalid data return 400 with validation errors
- [ ] Protected routes return 401 when unauthenticated
- [ ] Tenant-scoped routes return correct tenant data

---

## Gate 6 — No Leftover Debug Code

- [ ] No `Console.WriteLine()` in production paths
- [ ] No commented-out code blocks without explanation
- [ ] No hardcoded test values (use configuration or constants)
- [ ] Exception messages don't expose stack traces

---

## Final Gate — Ship Approval

- [ ] Gates 1-6 all passed
- [ ] Spec Confirmation matched implementation (see `AGENTS.md` workflow protocol)
- [ ] User confirmed ready to ship

**Only after all gates pass → run `ship.ps1`**
