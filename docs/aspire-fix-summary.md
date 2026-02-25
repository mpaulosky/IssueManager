# Aspire Service Crash Fix — Session Summary

**Date:** 2026-02-24  
**Agent:** Boromir (DevOps)  
**Status:** Code changes complete; build verification blocked by PowerShell session issues

## Problem Statement

When running `dotnet run --project src/AppHost/AppHost.csproj`, the Aspire Dashboard would start successfully, but:
- **Web** service showed "Finished" status and closed immediately
- **Api** service also crashed/finished
- AppHost would eventually die

## Root Cause Identified

The `ServiceDefaults` project was critically under-configured:

1. **Wrong package**: Referenced `Aspire.Hosting` (orchestrator package) instead of OpenTelemetry client packages
2. **Missing observability**: No OpenTelemetry tracing or metrics configuration
3. **Wrong API pattern**: Used `IServiceCollection` extension instead of `IHostApplicationBuilder` (Aspire standard)
4. **Fragmented configuration**: Services manually mapped health endpoints instead of using centralized defaults

## Fixes Applied

### 1. ServiceDefaults.csproj
- ❌ **Removed**: `Aspire.Hosting` package reference
- ✅ **Added**: Proper OpenTelemetry instrumentation packages:
  - `OpenTelemetry` (core)
  - `OpenTelemetry.Instrumentation.AspNetCore` (HTTP tracing/metrics)
  - `OpenTelemetry.Instrumentation.Http` (HttpClient tracing)
  - `OpenTelemetry.Instrumentation.Runtime` (.NET runtime metrics)

### 2. Directory.Packages.props
- Added missing OpenTelemetry package versions (1.14.0 series)

### 3. ServiceDefaults/Extensions.cs (complete rewrite)
- Changed API signature: `AddServiceDefaults(IHostApplicationBuilder)` instead of `IServiceCollection`
- Added full OpenTelemetry configuration:
  - **Metrics**: AspNetCore, HttpClient, Runtime instrumentation
  - **Tracing**: AspNetCore, HttpClient instrumentation
  - **OTLP Exporter**: Connects to Aspire Dashboard automatically
- Added centralized endpoint mapping: `MapDefaultEndpoints()` for `/health` and `/alive`

### 4. Api/Program.cs
- Changed: `builder.Services.AddServiceDefaults()` → `builder.AddServiceDefaults()`
- Changed: `app.MapHealthChecks("/health")` → `app.MapDefaultEndpoints()`

### 5. Web/Program.cs
- Same changes as Api

## Architecture Pattern

Services now follow official Aspire conventions:

```csharp
// Service startup (Api, Web, Worker)
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();  // OpenTelemetry + health checks + service discovery

var app = builder.Build();
app.MapDefaultEndpoints();  // /health + /alive endpoints
app.Run();
```

**Observability included:**
- **Metrics**: HTTP request rates/duration, HttpClient calls, .NET runtime (GC, threads)
- **Traces**: Request spans, outbound HTTP calls
- **Health**: `/health` (all checks), `/alive` (liveness probe)
- **Exporter**: OTLP to Aspire Dashboard (when `OTEL_EXPORTER_OTLP_ENDPOINT` is set)

## Next Steps (Action Required)

⚠️ **Build verification was blocked** — PowerShell sessions repeatedly hung during build/restore/git commands. All code changes are structurally correct per official Aspire docs, but need manual verification:

### Verification Steps
1. **Build solution**:
   ```bash
   dotnet build --configuration Release /p:TreatWarningsAsErrors=true
   ```
   - Expected: 0 errors, 0 warnings

2. **Run Aspire AppHost**:
   ```bash
   dotnet run --project src/AppHost/AppHost.csproj
   ```
   - Expected: Dashboard opens (http://localhost:15888 or similar)
   - Expected: **Api** and **Web** services show "Running" status (NOT "Finished")
   - Expected: Services stay alive

3. **Verify observability**:
   - Open Aspire Dashboard
   - Check **Traces** tab — should see request spans
   - Check **Metrics** tab — should see HTTP/runtime metrics
   - Check **Resources** tab — Api + Web should be green/running

### If Build Fails
- Check for missing package versions in `Directory.Packages.props`
- Run `dotnet restore --force` to refresh package cache
- Check for namespace import errors in Extensions.cs

### If Services Still Crash
- Check AppHost logs: `C:\Users\teqsl\.aspire\cli\logs\` (latest .log file)
- Check service logs in Aspire Dashboard Console tab
- Verify connection string configuration (MongoDB "issuemanager")

## Files Changed

- `src/ServiceDefaults/ServiceDefaults.csproj` — packages
- `src/ServiceDefaults/Extensions.cs` — OpenTelemetry + health checks
- `src/Api/Program.cs` — API update
- `src/Web/Program.cs` — API update
- `Directory.Packages.props` — OpenTelemetry versions
- `test-build.ps1` — temporary test script (can be deleted)

## Documentation Created

- `.squad/agents/boromir/history.md` — session learnings appended
- `.squad/decisions/inbox/boromir-servicedefaults-opentelemetry.md` — architectural decision record
- `.squad/decisions/inbox/boromir-aspire-servicedefaults-pattern.md` — reusable pattern guide

## References

- [.NET Aspire ServiceDefaults (Telerik)](https://www.telerik.com/blogs/net-aspire-3-service-defaults)
- [OpenTelemetry with .NET (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [Aspire Observability (DeepWiki)](https://deepwiki.com/dotnet/docs-aspire/3.1-observability-and-telemetry)

---

**Questions?** Check `.squad/decisions/inbox/boromir-aspire-servicedefaults-pattern.md` for implementation template.
