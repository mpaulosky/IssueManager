# Decision: Test Project Recovery (Critical Fix)

**Date:** 2026-02-18  
**Decider:** Aragorn  
**Status:** ✅ Resolved  

---

## Context

Gandalf's validation (I-10) discovered that 5 of 6 test projects were missing `.csproj` files:
- Unit
- Architecture  
- BlazorTests
- Integration
- Aspire

Only E2E.csproj existed and built. However, 97 test methods were already written across 15 test files, indicating that the test code was present but unbuildable.

**Impact:** Cannot build solution. Cannot run tests. CI/CD workflow fails immediately.

**Root Cause:** Test project scaffolding was incomplete during initial setup. Test code was generated, but the project files were never created or committed.

---

## Decision

Created all 5 missing .csproj files with correct:
- Target framework (net10.0)
- Test framework (xunit 2.9.3)
- Dependencies per test type
- Project references

Also:
- Updated IssueManager.slnx to register all 6 test projects
- Added/updated GlobalUsings.cs for common imports
- Upgraded MongoDB.Driver 3.2.0 → 3.5.2 to resolve dependency conflict

---

## Rationale

### Why these dependencies?

**Unit.csproj:**
- xunit: Test framework
- FluentAssertions: Readable assertions
- FluentValidation: Domain validators under test
- NSubstitute: Mocking dependencies

**Architecture.csproj:**
- NetArchTest.Rules: Architecture rules enforcement (layer boundaries, naming conventions)

**BlazorTests.csproj:**
- SDK: Microsoft.NET.Sdk.Web (required for Razor compilation)
- bunit: Blazor component test framework
- NSubstitute: Mock services injected into components

**Integration.csproj:**
- Testcontainers.MongoDb: Spin up isolated MongoDB for integration tests
- MongoDB.Driver: Direct database interaction for setup/assertions

**Aspire.csproj:**
- Aspire.Hosting: Test distributed app orchestration (DistributedApplication)

### Why upgrade MongoDB.Driver?

MongoDB.Entities 25.0.0 (used by Api) requires MongoDB.Driver >= 3.5.2.  
Directory.Packages.props had 3.2.0, causing NuGet NU1605 error (package downgrade).  
Upgraded to 3.5.2 to satisfy dependency graph.

---

## Alternatives Considered

1. **Delete test code, rebuild from scratch**  
   ❌ Wasteful. 97 test methods already written and correct.

2. **Create minimal .csproj files, defer dependencies**  
   ❌ Would require second pass to add dependencies. Do it right once.

3. **Use Directory.Build.props to centralize test config**  
   ⏸️ Future optimization. Current approach is explicit and unblocks immediately.

---

## Outcomes

### Build Verification (Successful)

✅ All 5 new test projects build  
✅ Solution restore completes  
✅ 70 tests discovered and run:
  - Unit: 30 passed
  - Architecture: 10 passed  
  - Blazor: 13 passed  
  - Integration: 17 passed (TestContainers working)
  - Aspire: 0 tests (placeholder structure ready)

❌ E2E: 31 tests fail (Playwright config issue — tracked separately, not Aragorn's domain)

### CI/CD Impact

✅ `dotnet build` now succeeds  
✅ `dotnet test` now runs 70 tests  
✅ Gandalf's validation (I-10) unblocked  

### Warnings (Non-blocking)

- NuGet NU1603: NSubstitute 5.2.0 not found, resolved to 5.3.0 (patch version bump, safe)
- NuGet NU1603: bunit 1.29.1 not found, resolved to 1.29.5 (patch version bump, safe)
- NuGet NU1902: KubernetesClient 15.0.1 has moderate vulnerability (tracked separately, not test-specific)
- NuGet NU1902: OpenTelemetry.Api 1.10.0 has moderate vulnerability (tracked separately, not test-specific)

---

## Lessons Learned

1. **Test project scaffolding is distinct from test code generation.**  
   Test methods can exist without .csproj files. Always verify solution build before marking as complete.

2. **SDK type matters for Blazor tests.**  
   bUnit requires `Microsoft.NET.Sdk.Web` (not `Microsoft.NET.Sdk`) to compile Razor components.

3. **Centralized package versions prevent drift.**  
   Directory.Packages.props helps, but must be kept in sync with dependency graphs (e.g., MongoDB.Entities transitively requires newer MongoDB.Driver).

4. **Integration tests are first-class citizens.**  
   TestContainers approach means integration tests run locally and in CI without mocking — real database behavior, real confidence.

---

## Next Steps

- ✅ Commit changes (done: 457e602)
- ⏸️ Gimli to document test project structure in TESTING.md
- ⏸️ Arwen to fix E2E Playwright configuration (31 failing tests)
- ⏸️ Legolas to address NuGet vulnerability warnings (KubernetesClient, OpenTelemetry.Api)

---

**Commit:** `457e602` — "fix: add missing test project .csproj files and solution references"
