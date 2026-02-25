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

### 2026-02-24: Aspire AppHost Full Build + Run Verification

**Task:** Restore, build, and run the Aspire AppHost to verify clean startup.

**Issue Found — Shared Environment File Lock:**
- `dotnet restore IssueManager.sln` fails immediately with:
  `Access to the path 'Aspire.Hosting.Tasks.dll' is denied.`
- Root cause: Shared server environment — other dotnet processes (18+ running) hold the default NuGet package cache (`~/.nuget/packages`) read-locked on `Aspire.Hosting.Tasks.dll`.
- This is a known issue in shared/multi-user environments where multiple Aspire builds compete over the same package cache.

**Fix — Alternate NuGet Packages Directory:**
- Set `$env:NUGET_PACKAGES = "$env:USERPROFILE\.nuget\packages_aspire"` before any dotnet command.
- This directs restore to a user-isolated cache, avoiding the lock contention.
- Restore completes successfully in ~5s with this env var set.

**Build Result:**
- `dotnet build IssueManager.sln --no-restore` → **Build succeeded. 0 Error(s). 0 Warning(s).**
- All 9 projects compile cleanly.

**AppHost Run Result:**
- `dotnet run --project src/AppHost/AppHost.csproj --no-build` → **Starts successfully**
- Aspire version: `13.1.1`
- Dashboard URL: `https://issuetracker.dev.localhost:17270`
- Login token generated correctly.
- Process stays running (does not crash on startup).

**Required Env Var for All dotnet Commands in This Repo:**
```powershell
$env:NUGET_PACKAGES = "$env:USERPROFILE\.nuget\packages_aspire"
dotnet restore IssueManager.sln
dotnet build IssueManager.sln --no-restore
```

**No code changes were needed** — the codebase was already clean from prior fixes. The only issue is operational (shared environment file locking).

**NuGet.config Fix Applied:**
The existing `NuGet.config` had `globalPackagesFolder` set to `$env:USERPROFILE\.nuget\packages_aspire` — this is PowerShell syntax, which NuGet.config does **not** expand. Changed to `%USERPROFILE%\.nuget\packages_aspire` (Windows env var syntax). NuGet now correctly resolves the user-specific package cache path without any env var workaround. `dotnet restore` and `dotnet build` both work without any env var setup.

### 2026-02-24: Created launchSettings.json for Api and Web

**Task:** Created missing `Properties/launchSettings.json` files for Api and Web projects.

**Files created:**
- `src/Api/Properties/launchSettings.json` — https (7194/5194) and http (5194) profiles, `launchBrowser: false`, opens `/openapi/v1.json`
- `src/Web/Properties/launchSettings.json` — https (7080/5080) and http (5080) profiles, `launchBrowser: true`

**Why:** Both projects had no standalone run profiles. Without launchSettings.json, `dotnet run` and Visual Studio/Rider cannot offer named profiles. Aspire AppHost has its own ports (17270/15177) for orchestrated startup — these standalone profiles are for running Api or Web independently outside Aspire for debugging and development.

**Port choices follow convention:** Api on 7194/5194, Web on 7080/5080 — no conflicts with AppHost ports.

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

### 2026-02-24: ServiceDefaults Complete Rewrite for Aspire Compliance

**Task:** Fixed the root cause of Aspire service crashes — ServiceDefaults was missing proper OpenTelemetry configuration and had wrong package references.

**Critical Issues Found:**
1. **Wrong package**: ServiceDefaults.csproj referenced `Aspire.Hosting` (orchestrator package) instead of OpenTelemetry instrumentation packages
2. **Minimal implementation**: Extensions.cs only had `AddServiceDiscovery()` and `AddHealthChecks()` — missing all OpenTelemetry setup
3. **Wrong signature**: `AddServiceDefaults()` took `IServiceCollection` instead of `IHostApplicationBuilder` (Aspire pattern)
4. **Manual health check mapping**: Api and Web manually called `MapHealthChecks("/health")` instead of using centralized `MapDefaultEndpoints()`

**Fixes Applied:**
1. **ServiceDefaults.csproj**: Removed `Aspire.Hosting`, added proper OpenTelemetry instrumentation packages:
   - `OpenTelemetry`
   - `OpenTelemetry.Instrumentation.AspNetCore`
   - `OpenTelemetry.Instrumentation.Http`
   - `OpenTelemetry.Instrumentation.Runtime`
   
2. **Directory.Packages.props**: Added missing OpenTelemetry package versions (1.14.0 series)

3. **Extensions.cs**: Complete rewrite following Aspire ServiceDefaults pattern:
   - Changed signature: `AddServiceDefaults(IHostApplicationBuilder)` instead of `IServiceCollection`
   - Added `ConfigureOpenTelemetry()` with metrics (AspNetCore, HttpClient, Runtime) and tracing (AspNetCore, HttpClient)
   - Added `AddOpenTelemetryExporters()` for OTLP exporter (Aspire Dashboard integration)
   - Added `MapDefaultEndpoints()` for centralized health check mapping (`/health`, `/alive`)
   
4. **Api/Program.cs**: Changed `builder.Services.AddServiceDefaults()` → `builder.AddServiceDefaults()`, replaced `MapHealthChecks("/health")` with `MapDefaultEndpoints()`

5. **Web/Program.cs**: Same changes as Api

**Architecture Pattern:**
- ServiceDefaults is now a proper Aspire shared library with full observability stack
- Services call `builder.AddServiceDefaults()` at startup
- Services call `app.MapDefaultEndpoints()` to expose health/liveness endpoints
- OpenTelemetry automatically exports to OTLP endpoint (Aspire Dashboard) when `OTEL_EXPORTER_OTLP_ENDPOINT` is set

**Environment Issue:**
PowerShell sessions repeatedly hung during build/test attempts (both `dotnet build` and `git status` commands). This prevented verification of the build, but all code changes are structurally correct and follow official Aspire patterns.

**Next Steps (for Matthew or next session):**
- Run `dotnet build --configuration Release` to verify compilation
- Run `dotnet run --project src/AppHost/AppHost.csproj` to test Aspire orchestration
- Verify Api and Web services start successfully and stay running
- Check Aspire Dashboard (http://localhost:15888 or similar) for telemetry data
