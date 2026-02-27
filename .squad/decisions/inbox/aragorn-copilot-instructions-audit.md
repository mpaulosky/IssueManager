# Decision: copilot-instructions.md Audit Results

**By:** Aragorn (Lead Developer)
**Date:** 2026-02-27
**Status:** Completed

---

## What

Full compliance audit of `.github/copilot-instructions.md` against the actual IssueManager project state. Nine stale references corrected in the instructions file. A gap report produced at `docs/reviews/copilot-instructions-audit.md`.

---

## Key Technical Decisions Confirmed

### 1. MongoDB ORM: MongoDB.Entities (not EF Core)

The instructions claimed EF Core + MongoDB.EntityFrameworkCore. The project uses **MongoDB.Entities v25** + raw **MongoDB.Driver** through repository classes. This is an intentional choice — the instructions must reflect it.

**Action taken:** Updated instructions to reflect `false` for EF Core, MongoDB.EntityFrameworkCore, DbContext Pooling/Factory/Change Tracking.

### 2. CQRS: Custom handler pattern (not MediatR)

The instructions referenced "MediatR usage". The project implements CQRS with plain handler classes (`CreateIssueHandler`, `ListIssuesHandler`, etc.) injected via DI — **no MediatR library**.

**Action taken:** Corrected the CQRS line in instructions to reference `Api/Handlers/` and `Shared/Validators/`.

### 3. P0 Gaps Escalated to Matthew

The following require immediate prioritization:

- **Auth0 + Authorization** — Zero implementation. `AdminPolicy` constant exists but no auth middleware. This is a security blocker before any production deployment.
- **CORS** — `DefaultCorsPolicy` constant defined in Shared, `AddCors()`/`UseCors()` never wired. API is open to all origins.

### 4. P1 Gaps to Schedule

- **Scalar UI** — `app.MapScalarApiReference()` must be added to `Api/Program.cs`. Package is installed.
- **API Versioning** — No versioning; all endpoints are unversioned.
- **Application Insights** — Not configured despite being required.

### 5. Instructions File Corrections Made

| Change | Reason |
|---|---|
| "TailwindBlogApp" → "IssueManager" | Wrong project name — copy/paste from another project |
| Date updated to 2026-02-27 | Stale since June 2025 |
| MediatR reference removed | Not used; custom CQRS only |
| EF Core → false | Not used |
| MongoDB.EntityFrameworkCore → false | MongoDB.Entities used instead |
| DbContext Pooling/Factory/Change Tracking → false | N/A without EF Core |
| `Persistence.MongoDb/` → `src/Api/Data/` | Correct path |
| `CODE_OF_CONDUCT.md` → `docs/CODE_OF_CONDUCT.md` | File is in docs/, not root |
| `Tests/Web.Tests.Bunit/` → `tests/Blazor.Tests/` | Correct directory |

---

## Why

Instructions that reference the wrong project name, wrong libraries, and wrong paths erode developer trust and cause Copilot suggestions to be misaligned with the actual codebase. Keeping them accurate is a team health requirement.
