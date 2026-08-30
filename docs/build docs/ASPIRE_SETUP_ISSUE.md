# Aspire AppHost Startup Issue - Investigation Summary

## Problem

The IssueManager AppHost fails to start with the following error:

```
Unhandled exception. System.AggregateException: One or more errors occurred. 
(Property CliPath: The path to the DCP executable used for Aspire orchestration is required.; 
Property DashboardPath: The path to the Aspire Dashboard binaries is missing.)
```

**Location:** `src/AppHost/Program.cs:15` → `builder.Build().Run()`

## Root Cause Analysis

### Investigation Findings

1. **.NET 10 SDK**: ✅ Installed and correct (`dotnet --version`)
2. **Aspire NuGet Packages**: ✅ Present in project (`Aspire.Hosting.AppHost`, `Aspire.Hosting.MongoDB` v13.1.1)
3. **Aspire Workload**: ⚠️ Deprecated (now NuGet-only, not workload-based)
4. **Aspire CLI**: ⚠️ Installed but incomplete
5. **DCP Executable**: ❌ **MISSING** from NuGet package cache
6. **Docker**: ✅ Available for MongoDB container orchestration

### Why DCP is Missing

This is a known issue in .NET Aspire versions 9.5.1+ where:
- The Aspire orchestration packages (`aspire.hosting.orchestration.win-x64`) are incomplete
- The `dcp.exe` file is missing from the NuGet package
- Installation via `aspire.dev/install.ps1` does not include necessary DCP binaries

**References:**
- [GitHub Issue: dcp.exe missing in NuGet package](https://github.com/dotnet/aspire/issues/11866)
- [Visual Studio Developer Community: Aspire fails after upgrade](https://developercommunity.visualstudio.com/t/Aspire-fails-to-run-after-VS-upgrade---/10994705)

## Attempted Solutions

### ✅ Completed
1. Verified .NET 10.0.100 SDK installation
2. Confirmed Aspire NuGet packages v13.1.1 in project
3. Installed Aspire CLI via `irm https://aspire.dev/install.ps1 | iex`
4. Verified `aspire.exe` location: `C:\Users\teqsl\.aspire\bin\aspire.exe`
5. Checked AppHost configuration - **valid** (no configuration issues)

### ❌ Failed/Incomplete
1. `dotnet workload install aspire` - Aspire workload is deprecated (returns success but no actual workload)
2. `dotnet nuget locals all --clear` - Permissions errors clearing cache
3. `dotnet new install Aspire.ProjectTemplates --force` - Hangs during installation
4. Direct DCP executable not found in NuGet cache (`E:\teqsl\.nuget\packages\aspire.hosting.orchestration*`)

## Configuration Details

### AppHost Structure
- **File:** `src/AppHost/Program.cs`
- **Configuration:** MongoDB + API + Web projects (valid setup)
- **Launch Settings:** Hard-coded dashboard URLs (potential secondary issue)

```csharp
var builder = DistributedApplication.CreateBuilder(args);
var mongodb = builder.AddMongoDB("mongodb").AddDatabase("issuemanager");
var api = builder.AddProject("api", "../Api/Api.csproj").WithReference(mongodb);
builder.AddProject("web", "../Web/Web.csproj").WithReference(api);
builder.Build().Run();  // ← Fails here with missing DCP
```

### LaunchSettings Issue
- Dashboard endpoints are **hard-coded** in `Properties/launchSettings.json` (lines 12-14, 25-27)
- Should allow Aspire to auto-configure these endpoints

## Recommended Next Steps

### Immediate Workarounds

1. **Option A: Manual DCP Installation** (if available)
   - Download compatible `dcp.exe` from a working Aspire installation
   - Place in: `E:\teqsl\.nuget\packages\aspire.hosting.orchestration.win-x64\<version>\tools\dcp.exe`

2. **Option B: Use Docker Compose Directly**
   - Run MongoDB container manually
   - Run individual projects without Aspire orchestration
   - **Downside:** Loses unified orchestration and dashboard benefits

3. **Option C: Switch to Aspire Dashboard Container**
   - Some users report success running Aspire Dashboard as a Docker container
   - Still requires DCP executable for orchestration

### Long-Term Solutions

1. **Upgrade Aspire Packages**
   - Wait for patched version that includes DCP binaries
   - Monitor: https://github.com/dotnet/aspire/issues/11866

2. **Use Dev Container**
   - Run entire development environment in Docker
   - Avoids local DCP installation requirements

3. **Update Prerequisites Documentation**
   - Add explicit note about DCP requirement
   - Add troubleshooting section to README.md

## Documentation Updates Needed

### `README.md` (Prerequisites section, line 22)
**Current:**
```
Prerequisites: .NET 10 SDK, Docker (for MongoDB)
```

**Proposed:**
```
Prerequisites:
- .NET 10.0 SDK
- Docker Desktop or Podman (for MongoDB orchestration)
- .NET Aspire DCP (Distributed Cloud Platform) — see ASPIRE_SETUP_ISSUE.md for troubleshooting
```

### New File: `.github/ASPIRE_SETUP.md`
Should document:
- Aspire installation steps
- DCP executable requirements
- Dashboard access URL
- Common troubleshooting (this issue + solutions)

## Current Environment State

- ✅ .NET SDK: `10.0.100`
- ✅ Docker: Running (verified)
- ⚠️ Aspire CLI: Installed (incomplete)
- ❌ DCP Executable: Missing
- ❌ Aspire Dashboard Binaries: Missing

---

**Status:** BLOCKED - Requires DCP executable or alternative setup
**Assigned to:** Team (Boromir or Aragorn for platform setup)
**Follow-up:** Monitor GitHub issue #11866 for DCP binary patch
