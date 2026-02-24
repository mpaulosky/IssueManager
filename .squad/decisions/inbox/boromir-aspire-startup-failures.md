# Aspire Service Startup Failures — Fixes Applied

**Date:** 2026-02-24  
**Author:** Boromir (DevOps)  
**Status:** Fixed

## Problem

Api and Web services fail to start when running the Aspire AppHost. Both services crash immediately on startup.

## Root Cause

**Primary Issue:** ServiceDefaults incomplete implementation  
`src/ServiceDefaults/Extensions.cs:AddServiceDefaults()` does not register health checks, but both Api and Web projects call `app.MapHealthChecks("/health")` which requires health checks to be available in DI.

**Error Message:**
```
System.InvalidOperationException: Unable to find the required services. 
Please add all the required services by calling 'IServiceCollection.AddHealthChecks' 
inside the call to 'ConfigureServices(...)' in the application startup code.
   at Program.<Main>$(String[] args) in E:\github\IssueManager\src\Api\Program.cs:line 134
```

## Additional Issues

1. **Incorrect Package References:**
   - `Api.csproj` and `Web.csproj` both reference `Aspire.Hosting.AppHost` 
   - This is the orchestrator package and should NOT be in service projects
   - Api needs `Aspire.MongoDB.Driver` for MongoDB client integration

2. **Connection String Naming:**
   - AppHost defines: `.AddDatabase("issuemanager")` (lowercase)
   - Api expects: `GetConnectionString("IssueManagerDb")`
   - Aspire injects as: `"issuemanager"` (matching the database name)

## Fixes Applied

### Fix 1: ServiceDefaults — AddHealthChecks ✅
Added `services.AddHealthChecks();` to `AddServiceDefaults()` in `src/ServiceDefaults/Extensions.cs`. Both Api and Web map health check endpoints, so health checks must be registered in DI.

### Fix 2: Removed Incorrect Package References ✅
- Removed `Aspire.Hosting.AppHost` from `src/Api/Api.csproj` — orchestrator-only package.
- Removed `Aspire.Hosting.AppHost` from `src/Web/Web.csproj` — orchestrator-only package.

### Fix 3 (bonus): Suppressed ASPIRE004 Warning ✅
Added `IsAspireProjectResource="false"` to the ServiceDefaults project reference in `src/AppHost/AppHost.csproj`. ServiceDefaults is a shared library, not a launchable Aspire service.

### Build Result ✅
Zero errors, zero warnings. All 9 projects compile cleanly in Release configuration.

## Notes

- MongoDB container confirmed running: `mongodb-bjuzadmx` on port 62201
- Both projects build successfully in isolation
- NuGet workaround still required: `$env:NUGET_PACKAGES = "$env:USERPROFILE\.nuget\packages_aspire"`
