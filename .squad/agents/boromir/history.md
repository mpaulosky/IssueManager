# Boromir — History

## Core Context
DevOps on IssueManager (.NET 10, GitHub Actions, Aspire, NuGet centralized packages). User: Matthew Paulosky.

## Learnings

### 2026-02-25: NuGet.config cross-platform fix
- Removed Windows-only `<config>` block from `NuGet.config` that set `globalPackagesFolder` to `%USERPROFILE%\.nuget\packages_aspire`
- This caused `MSB4019` error on Linux CI runners (path not expanded)
- Fixed in commit `26b3e73` on PR #54

### 2026-02-25: Protected Branch Guard
- Workflow: `.github/workflows/` (guard checks `.squad/` files in PR diff on non-exempt branches)
- Only `squad/*` branches are exempt from the guard
- `feature/*` branches: if `.squad/` files appear in the diff, the CI check fails

### 2026-02-25: AppHost Logging Pattern
- For static extension methods in AppHost (e.g., `AddMongoDbServices`, `AddRedisServices`), use `LoggerFactory.Create(b => b.AddConsole())` to bootstrap a logger
- Pattern: `using var loggerFactory = LoggerFactory.Create(b => b.AddConsole()); var logger = loggerFactory.CreateLogger<TClass>();`
- Use structured logging with named parameters: `logger.LogInformation("Message with {Param}", value);`
- `LogInformation` for key lifecycle events (environment detection, resource creation)
- `LogDebug` for detailed configuration steps (resource configuration details)
- This is the most pragmatic approach for AppHost startup configuration where DI is not yet available

### 2026-06-12: NuGet Package Upgrade Run

**Packages upgraded:**
- Aspire.\* (all 6): 13.1.1 → 13.1.2
- Auth0.AspNetCore.Authentication: 1.5.0 → 1.6.1
- Microsoft.AspNetCore.Authentication.JwtBearer: 10.0.0 → 10.0.3
- OpenTelemetry.\* (all 7): 1.14.0 → 1.15.0
- Microsoft.AspNetCore.OpenApi: 10.0.0 → 10.0.3
- Microsoft.AspNetCore.Components.Web: 10.0.0 → 10.0.3
- Microsoft.Extensions.ServiceDiscovery: 10.0.0 → 10.3.0
- MongoDB.Bson + MongoDB.Driver: 3.5.2 → 3.6.0 (kept in sync)
- Scalar.AspNetCore: 1.2.51 → 2.12.50 (MAJOR version jump — Gimli/Legolas must verify API reference calls)
- Asp.Versioning.Http: 8.1.0 → 8.1.1
- xunit.runner.visualstudio: 3.0.0 → 3.1.5
- Microsoft.NET.Test.Sdk: 17.13.0 → 18.3.0
- Coverlet.Collector: 6.0.0 → 8.0.0
- Testcontainers + Testcontainers.MongoDB: 3.10.0 → 4.10.0 (MAJOR version jump — Gimli must verify integration tests compile)
- Microsoft.Playwright: 1.50.1 → 1.58.0
- bunit: 1.29.5 → 2.6.2 (MAJOR version jump — Gimli must run bunit-test-migration skill)

**Packages at latest stable (no change):**
- FluentValidation: 12.1.1
- MongoDB.Entities: 25.0.0
- NSubstitute: 5.3.0
- NetArchTest.Rules: 1.3.2
- xunit: 2.9.3 (held at 2.x)

**Packages intentionally held back:**
- **xunit**: stays on 2.9.3 (latest 2.x). v3 is a breaking change requiring explicit team approval.
- **FluentAssertions**: stays on 6.12.1 (latest 6.x). v7+ changed to a commercial license and has breaking API changes.

### Key files
- `Directory.Packages.props` — all NuGet package versions
- `NuGet.config` — must be cross-platform (no `%USERPROFILE%` paths)
- `.github/workflows/` — GitHub Actions CI definitions
- `src/AppHost/` — Aspire AppHost project
- `src/ServiceDefaults/` — Aspire ServiceDefaults project
