# Copilot Instructions Audit — IssueManager

**Audited by:** Aragorn (Lead Developer)
**Date:** 2026-02-27
**Reference:** `.github/copilot-instructions.md` vs actual project state

---

## ✅ COMPLIANT

Requirements the project already satisfies:

| Category | Requirement | Evidence |
|---|---|---|
| Tech Stack | .NET 10, C# 14 | All `.csproj` files: `<TargetFramework>net10.0</TargetFramework>`, `<LangVersion>14.0</LangVersion>` |
| Tech Stack | SDK `10.0.100` in `global.json` | `global.json` with rollForward: latestMinor |
| C# Style | `.editorconfig` | Root `.editorconfig` with tab/size-2/lf/utf-8/trim/final-newline |
| C# Style | File-scoped namespaces | All `.cs` files use `namespace X;` syntax |
| C# Style | Global usings | `GlobalUsings.cs` present in Api, Shared, test projects |
| C# Style | Nullable reference types | `<Nullable>enable</Nullable>` in every `.csproj` |
| C# Style | Pattern matching | Used throughout source code |
| C# Style | TailwindCSS | Used in all Blazor component markup |
| Security | HTTPS | `UseHttpsRedirection()` in Api and Web `Program.cs` |
| Security | Antiforgery tokens | `UseAntiforgery()` in `Web/Program.cs` |
| Architecture | Aspire | `AppHost/` project wires MongoDB + Redis; `AddServiceDefaults()` called |
| Architecture | DI | `AddRepositories()`, `AddValidators()`, `AddHandlers()` in `ServiceCollectionExtensions.cs` |
| Architecture | Async/Await | All repository methods are `async Task<T>` |
| Architecture | VSA | Handlers organized by feature: `Api/Handlers/Issues/`, `/Categories/`, `/Statuses/`, `/Comments/` |
| Architecture | Centralized NuGet | `Directory.Packages.props` with `ManagePackageVersionsCentrally=true` |
| Architecture | Unit Tests | `tests/Unit.Tests/` — 368+ passing tests |
| Architecture | Integration Tests | `tests/Integration.Tests/` with TestContainers.MongoDB |
| Architecture | Architecture Tests | `tests/Architecture.Tests/` with NetArchTest.Rules |
| Blazor | Interactive Server Rendering | `AddInteractiveServerComponents()` + `AddInteractiveServerRenderMode()` |
| Documentation | OpenAPI | `Microsoft.AspNetCore.OpenApi` installed; `app.MapOpenApi()` in Api |
| Documentation | XML Docs | `<summary>` tags on public types/methods in ServiceDefaults, Api/Data, Shared |
| Documentation | README | `README.md` at root |
| Documentation | CONTRIBUTING.md | `docs/CONTRIBUTING.md` |
| Documentation | LICENSE | `LICENSE` at root |
| Documentation | CODE_OF_CONDUCT.md | `docs/CODE_OF_CONDUCT.md` |
| Documentation | SECURITY.md | `SECURITY.md` at root |
| Logging | OpenTelemetry | Metrics + tracing configured in `ServiceDefaults/Extensions.cs`; OTLP exporter |
| Logging | Health checks | `AddDefaultHealthChecks()`; `/health` and `/alive` endpoints |
| Database | MongoDB | `MongoDB.Driver` + `MongoDB.Entities` in use; repositories in `Api/Data/` |
| Database | Async operations | All repository methods async |
| Database | TestContainers | `Testcontainers.MongoDB` in `Integration.Tests.csproj` |
| Database | No in-memory DB | No in-memory database packages referenced |
| File Placement | Working docs in `docs/` | All planning docs in `docs/`; no `.log`/`.txt` at root |
| Validation | FluentValidation | `FluentValidation` package; validators for all 4 aggregate types |
| Testing | xUnit | In `Directory.Packages.props`; used in all test projects |
| Testing | FluentAssertions | In `Directory.Packages.props`; used in all test projects |
| Testing | NSubstitute | In `Directory.Packages.props`; used in Unit.Tests |
| Testing | bUnit | In `Directory.Packages.props`; used in `tests/Blazor.Tests/` |
| Testing | TestContainers | Integration test project + MongoDB TestContainers fixture |
| Testing | Playwright (package) | `Microsoft.Playwright` in `Directory.Packages.props` |
| File Copyright | Partial | Present in `AppHost/`, `Api/Data/`, `Shared/Constants/` |

---

## ⚠️ PARTIAL

Requirements partially implemented — some gap exists:

| Category | Requirement | What's Present | What's Missing |
|---|---|---|---|
| Documentation | Scalar interactive UI | Package `Scalar.AspNetCore` in `.csproj` | `app.MapScalarApiReference()` never called in `Api/Program.cs` — UI not reachable |
| Architecture | CQRS | Manual handler pattern (Commands/Queries as request objects, Handler classes) | MediatR library NOT installed; instructions claim "MediatR usage" — actually custom CQRS |
| Security | CORS | `Constants.DefaultCorsPolicy` string defined | `AddCors()` / `UseCors()` never called in any `Program.cs` |
| Security | Secure headers | HSTS configured via `UseHsts()` | No security headers middleware (CSP, X-Frame-Options, etc.) |
| Blazor | Component naming | Some correct (`IssueForm`) | Layouts/nav components don't carry `Component` suffix; `Home.razor` not named `HomePage.razor` |
| Blazor | Error boundaries | No `<ErrorBoundary>` in `MainLayout.razor` | Instructions claim it's there — it is not |
| Blazor | Stream rendering | None used | `@attribute [StreamRendering]` absent from all pages |
| Blazor | Virtualization | None used | `<Virtualize>` component absent |
| Blazor | Cascading parameters | None explicit | Instructions require it |
| Blazor | Render fragments | None explicit | Instructions require it |
| Caching | Output caching | Redis provisioned in AppHost; `OutputCache` constant defined | `AddOutputCache()` / `UseOutputCache()` not wired in `Web/Program.cs` |
| Caching | Distributed cache | Redis provisioned in AppHost | `AddStackExchangeRedisCache()` or similar not called in `Web/Program.cs` |
| Middleware | Request logging | None explicit | No request logging middleware configured |
| Logging | Structured logging | OpenTelemetry traces/metrics | No explicit structured log provider (Serilog/Seq) beyond default .NET logging |
| Testing | Playwright | Package referenced | No dedicated Playwright test project exists |
| File Copyright | Copyright headers | Present in ~30% of source files | Absent in most Api handlers, Web components, test files |
| Documentation | File header template | `.editorconfig` has `file_header_template = unset` | StyleCop SA1633/SA1636 disabled — no enforced copyright header mechanism |
| Environment | User Secrets | `.gitignore` excludes secret files | No `<UserSecretsId>` in any project file |

---

## ❌ MISSING

Requirements with zero implementation:

| Category | Requirement | Priority | Rationale |
|---|---|---|---|
| Security | Auth0 Authentication | **P0** | No auth library, no middleware, no policies wired. Entire auth system is absent. |
| Security | Authorization | **P0** | No `AddAuthorization()`, no policies, no `[Authorize]` attributes. `AdminPolicy` constant exists but is never applied. |
| Logging | Application Insights | **P1** | No `Azure.Monitor.OpenTelemetry.AspNetCore` or `Microsoft.ApplicationInsights.AspNetCore` package. |
| Versioning | API Versioning | **P1** | No `Asp.Versioning.Http` package; no versioned routes; all endpoints are unversioned. |
| Background Services | IHostedService/BackgroundService | **P2** | No background service implementations anywhere in `src/`. |
| Environment | Key Vault | **P2** | No `Azure.Extensions.AspNetCore.Configuration.Secrets` package or `AddAzureKeyVault()`. |
| Environment | User Secrets (configured) | **P2** | No `<UserSecretsId>` in `.csproj` files. |
| Database | MongoDB.EntityFrameworkCore | **P2** | Instructions require EF Core + MongoDB.EntityFrameworkCore; project uses `MongoDB.Entities` instead — fundamentally different ORM. |
| Database | EF Core (DbContext, Pooling, Factory, Change Tracking) | **P2** | Not used. Project uses raw MongoDB.Driver through MongoDB.Entities. |
| Versioning | Semantic versioning | **P3** | No `<Version>` in project files; no version tags or release configuration. |

---

## Top 10 Remediation Items

Prioritized by: Security → Architecture → Quality → Docs

| # | Item | Priority | Category | Action |
|---|---|---|---|---|
| 1 | **Implement Auth0 authentication** | P0 🔴 | Security | Install `Auth0.AspNetCore.Authentication`, configure in `Web/Program.cs` and `Api/Program.cs`. Add `[Authorize]` and wire `AdminPolicy`. |
| 2 | **Wire CORS policy** | P0 🔴 | Security | Add `builder.Services.AddCors(...)` using `Constants.DefaultCorsPolicy` and `app.UseCors()` in `Api/Program.cs`. |
| 3 | **Wire Scalar UI** | P1 🟠 | Docs/API | Add `app.MapScalarApiReference()` in `Api/Program.cs` so the Scalar UI is actually accessible. |
| 4 | **Add API Versioning** | P1 🟠 | Architecture | Install `Asp.Versioning.Http`; version all API endpoints (e.g., `/api/v1/issues`). |
| 5 | **Add Application Insights** | P1 🟠 | Observability | Install `Azure.Monitor.OpenTelemetry.AspNetCore`; hook into existing OpenTelemetry pipeline in `ServiceDefaults`. |
| 6 | **Add Output Caching middleware** | P2 🟡 | Caching | `builder.Services.AddOutputCache()` + `app.UseOutputCache()` in `Web/Program.cs`. Redis integration via Aspire. |
| 7 | **Clarify MongoDB ORM approach** | P2 🟡 | Architecture | Instructions say EF Core + MongoDB.EntityFrameworkCore. Project uses MongoDB.Entities. Decision needed: adopt EF Core or update instructions to reflect MongoDB.Entities. |
| 8 | **Add a Background Service** | P2 🟡 | Architecture | Implement at least one `BackgroundService` (e.g., cache warming, archival job) in `Api/` or `Web/`. |
| 9 | **Configure User Secrets and Key Vault** | P2 🟡 | Config | Add `<UserSecretsId>` to `Api.csproj` and `Web.csproj`. Add Key Vault integration for production secrets. |
| 10 | **Add Playwright test project** | P3 🟢 | Testing | Create `tests/Playwright.Tests/` project using the already-referenced `Microsoft.Playwright` package. |

---

## Instructions File Corrections Required

The `.github/copilot-instructions.md` file contains several stale or inaccurate references that must be corrected:

| Line | Current Text | Problem | Correct Text |
|---|---|---|---|
| Header | "Last updated: June 12, 2025" | Stale date | Update to 2026-02-27 |
| Line 6-7 | "…conventions in the **TailwindBlogApp** solution" | Wrong project name | "…conventions in the **IssueManager** solution" |
| Architecture section | "CQRS (see `Domain/Abstractions/`, **MediatR** usage)" | No MediatR; no Domain layer | "CQRS — custom handler pattern (see `Api/Handlers/`, `Shared/Validators/`)" |
| Database section | "Use **Entity Framework Core:** true" | EF Core not used | Clarify: project uses `MongoDB.Entities` + `MongoDB.Driver` directly |
| Database section | "Use **MongoDB.EntityFrameworkCore:** true" | Not used | Remove or mark as aspirational |
| Database section | "Use **DbContext Pooling/Factory/Change Tracking:** true" | N/A without EF Core | Remove until EF Core is adopted |
| Database section | "see `Persistence.MongoDb/`" | No such path | "see `src/Api/Data/`" |
| Documentation section | "see `CODE_OF_CONDUCT.md`" | File is in `docs/` | "see `docs/CODE_OF_CONDUCT.md`" |
| Testing section | "see `Tests/Web.Tests.Bunit/`" | Wrong path | "see `tests/Blazor.Tests/`" |
| Blazor section | "Use Error Boundaries: true (see MainLayout.razor error UI)" | Not implemented | Mark as aspirational or remove the evidence reference |

---

## Summary

The **IssueManager** project has a solid architectural foundation: .NET 10, Vertical Slice Architecture, Aspire orchestration,
OpenTelemetry, FluentValidation, and a complete test pyramid (unit, integration, architecture, bUnit). The most critical
gap is the **complete absence of authentication/authorization** — Auth0 is claimed but not implemented. The second most
impactful fix is **wiring Scalar** (the package is installed but the UI endpoint is not mapped). The instructions file
itself contains stale references to "TailwindBlogApp", MediatR, and EF Core that should be corrected to reflect the
actual technology choices in this codebase.
