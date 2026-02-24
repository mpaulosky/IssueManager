# Project Context

- **Owner:** Matthew Paulosky
- **Project:** IssueManager — issue management app with .NET 10, Blazor, MongoDB, CQRS, Vertical Slice Architecture
- **Stack:** .NET 10, C# 14, Blazor (server-side), Aspire, MongoDB.EntityFrameworkCore, MediatR (CQRS), FluentValidation, xUnit, FluentAssertions, NSubstitute, bUnit, TestContainers, GitHub Actions
- **My Role:** DevOps / Infrastructure Engineer — GitHub Actions, CI/CD, Aspire, Docker, builds, releases
- **Key Paths:** `.github/workflows/`, `src/AppHost/`, `scripts/`, `Directory.Packages.props`, `global.json`
- **Created:** 2026-02-24

## Learnings

### Established Process (inherited from prior sessions)

- Feature branches mandatory for all changes — direct pushes to main are prohibited
- Build-repair skill at `.squad/skills/build-repair/SKILL.md` must run before any push
- Release automation implemented via `.github/release.yml` + `squad-release.yml` (tag-triggered, `--generate-notes`)
- Tests renamed: `tests/Unit.Tests/`, `tests/Architecture.Tests/`, `tests/Blazor.Tests/`, `tests/Integration.Tests/`
- Integration tests use TestContainers + MongoDB 8.0 — Docker must be running, tests take ~105s

<!-- Append new learnings below. -->

### 2026-02-24: Aspire Service Startup Investigation

**Task:** Diagnosed Aspire AppHost service failures for Api and Web projects.

**Root Cause Identified:**
1. **ServiceDefaults missing health checks registration**: `AddServiceDefaults()` in `src/ServiceDefaults/Extensions.cs` does not call `AddHealthChecks()`, but both Api and Web call `app.MapHealthChecks("/health")` which requires health checks to be registered first.
   - Error: `System.InvalidOperationException: Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddHealthChecks'`

**Secondary Issues Found:**
2. **Incorrect package references**: 
   - `Api.csproj` references `Aspire.Hosting.AppHost` (orchestrator package, should be client package)
   - `Web.csproj` references `Aspire.Hosting.AppHost` (should not reference any Aspire hosting package)
   - Api should use `Aspire.MongoDB.Driver` or `Aspire.MongoDB.EntityFrameworkCore` for MongoDB client integration

3. **Connection string mismatch potential**:
   - AppHost defines database as `"issuemanager"` (lowercase)
   - Api looks for connection string `"IssueManagerDb"` with fallback to `"mongodb://localhost:27017"`
   - Aspire injects connection string as `"issuemanager"` (matching database name from AppHost)

**Environment verified:**
- Both projects build successfully standalone
- MongoDB container running (docker container `mongodb-bjuzadmx`)
- AppHost builds and starts dashboard successfully
- NuGet workaround required: `$env:NUGET_PACKAGES = "$env:USERPROFILE\.nuget\packages_aspire"`

**Fix Priority:**
1. Add `services.AddHealthChecks()` to `ServiceDefaults.AddServiceDefaults()` method
2. Fix package references in Api.csproj and Web.csproj
3. Add Aspire MongoDB client integration package to Api project
4. Verify connection string configuration matches Aspire naming

### 2026-02-24: Aspire Service Startup Fixes Applied

**Fixes applied (all three):**
1. **ServiceDefaults/Extensions.cs** — Added `services.AddHealthChecks();` to `AddServiceDefaults()` so health check endpoints work in Api and Web.
2. **Api.csproj** — Removed incorrect `Aspire.Hosting.AppHost` package reference (orchestrator-only package).
3. **Web.csproj** — Removed incorrect `Aspire.Hosting.AppHost` package reference.
4. **AppHost.csproj** — Added `IsAspireProjectResource="false"` to the ServiceDefaults project reference to suppress ASPIRE004 warning (ServiceDefaults is a shared library, not a launchable service).

**Build result:** Zero errors, zero warnings. All 9 projects compile cleanly in Release configuration.

**Key learning:** `Aspire.Hosting.AppHost` must ONLY be referenced by the AppHost orchestrator project. Service projects (Api, Web) should never reference it — they consume Aspire client integration packages instead.
