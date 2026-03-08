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

### 2026-02-28: FluentAssertions Upgrade to v8+
- FluentAssertions upgraded from 6.12.1 → 8.8.0 (latest stable v7+ version)
- Project confirmed non-commercial by Matthew Paulosky — commercial license concern resolved
- v7+ breaking changes to async assertion API: .Should().CompleteWithinAsync() now returns Task<AndConstraint<...>> instead of Assertion<...>
- Tests using async assertions will need updates (e.g., .Should().CompleteWithinAsync(...).Should()... patterns changed)
- Updated in Directory.Packages.props line 43

### 2026-02-28: xUnit v3 Migration (3.2.2)
- **Package:** xUnit 2.9.3 → xunit.v3 3.2.2 (latest stable)
- **Breaking Changes Fixed:**
  - `IAsyncLifetime` return types: `Task` → `ValueTask` (11 test classes affected)
  - `Xunit.Abstractions` namespace: types moved to `Xunit` namespace (no instances found)
  - `TestContext` ambiguity: xUnit v3 introduces its own TestContext type, conflicting with bUnit's TestContext
    - Solution: Fully qualified all bUnit TestContext references as `Bunit.TestContext`
    - Files affected: ComponentTestBase.cs + 6 page test files
- **Files Modified:**
  - Directory.Packages.props (package version)
  - All 4 test .csproj files (package reference name)
  - MongoDbFixture.cs + 10 integration test classes (IAsyncLifetime)
  - ComponentTestBase.cs + 6 Blazor page test files (TestContext qualification)
- **Build Status:**
  - ✅ Unit.Tests: compiles successfully
  - ✅ Integration.Tests: compiles successfully
  - ✅ Architecture.Tests: compiles successfully
  - ⚠️ Blazor.Tests: 117 CS0619 warnings (bUnit deprecated API usage) + 1 CS1061 error (bUnit breaking change)
    - These are bUnit 2.6.2 API changes, NOT xUnit v3 issues
    - Requires separate bUnit migration (RenderComponent → Render, SetParametersAndRender removed)
    - Gimli must run bunit-test-migration skill

### 2026-02-28: Pre-Push Hook Enhancement — Three-Gate Strategy
- **Context:** Matthew Paulosky requested adding two new gates to `scripts/hooks/pre-push` BEFORE the existing test suite
- **Gates Added:**
  1. **Copyright Header Validation** (Gate 1, fastest):
     - Scans all `.cs` files in `src/` and `tests/` directories
     - For files with full header format (detected by `// File Name :` presence), validates:
       - File Name header matches actual filename
       - Solution Name header equals "IssueManager"
       - Project Name header matches expected project name based on directory path
     - Path-to-project mapping: src/Api → Api, src/Web → Web, tests/Unit.Tests → Unit Tests, etc.
     - Skips files with simple one-liner copyright (no `// File Name :`)
     - Reports: files checked count + any failures with file path and field mismatch
  2. **Code Formatting Check** (Gate 2, medium speed):
     - Runs `dotnet format IssueManager.sln --verify-no-changes --verbosity quiet`
     - Blocks push if any files would be reformatted
     - User must run `dotnet format` locally to fix before pushing
  3. **Test Suite Execution** (Gate 3, slowest — unchanged):
     - Runs Unit.Tests, Blazor.Tests, Architecture.Tests in Release mode
     - Integration.Tests excluded (require Docker/TestContainers)
- **Implementation Pattern:**
  - All three gates use `FAILED=1` accumulation (don't abort on first failure, report everything)
  - Ordered fastest → slowest for quick feedback
  - Added detailed comment block explaining all three gates
  - Bash syntax validated with `bash -n`
  - Line endings: LF (Unix) for cross-platform compatibility
- **Commit:** `094dab7` on main
- **Files Modified:** `scripts/hooks/pre-push` (complete rewrite with three gate functions)


### 2026-03-04: Issue #89 — Aspire Startup Fixes: Blocked by Incomplete ObjectId Refactoring

**Attempted:** Commit Matthew's Aspire startup fixes for #89

**Status:** BLOCKED — Build fails with 14 compilation errors

**Root Cause:**
- Working tree contains TWO separate concerns mixed together:
  1. **Aspire infrastructure fixes** (AppHost, ServiceDefaults, API/Web startup) ✅ Sound
  2. **ObjectId type refactoring** (across all layers) ❌ INCOMPLETE
- ObjectId type changes made to DTOs/validators but NOT propagated to handlers, services, pages
- Type conversion logic missing (string ↔ ObjectId marshaling)

**Build Errors:**
- 9 validator files: ObjectId initialized with \string.Empty\ (type mismatch)
- API handlers: string parameters where ObjectId expected (CategoryEndpoints, StatusEndpoints, CommentEndpoints, UpdateIssueStatusHandler)
- Web pages: string literals passed to ObjectId parameters (EditIssuePage, EditCategoryPage, EditStatusPage, IssueDetailPage)

**Scope Issue:**
- ObjectId refactoring is application logic (belongs to Sam/Aragorn, not DevOps)
- Affects handlers, services, page components across entire application
- Requires coordinated type conversion implementation

**Decision:**
- Do NOT commit incomplete application refactoring
- Escalated to .squad/decisions/inbox/boromir-issue89-incomplete-refactoring.md
- Request: Matthew/application team must complete refactoring, test locally, then resubmit

### 2026-03-04: CI Test Failures — _Imports.razor Copyright Header Placement

**Issue:** GitHub CI Test Suite workflow failing with 8 CS0103 errors on `FooterComponent.razor` after copyright header sweep by Aragorn and Gimli.

**Root Cause:**
- Aragorn's commit `acd39d6` added copyright header to `src/Web/_Imports.razor` BEFORE the `@using` directives
- In Razor files, copyright comments that reference imported types CANNOT precede the `@using` directives that import those types
- When copyright header was placed at line 1-8, all subsequent `@using` statements were parsed but the BuildInfo class (from `@using Web`) was not accessible in FooterComponent.razor
- Result: 8 errors like `error CS0103: The name 'BuildInfo' does not exist in the current context`

**Fix:**
- Moved copyright header in `_Imports.razor` to END of file (after all `@using` directives)
- For `_Imports.razor` specifically: copyright header must come AFTER imports, not before
- Build + all tests now pass locally (Unit.Tests: 417 passed, Blazor.Tests: 164 passed, Architecture.Tests: 9 passed, Aspire.Tests: 18 passed)

**Pre-Push Hook Validation:**
- Pre-push hook in `scripts/hooks/pre-push` already enforces Unit.Tests + Blazor.Tests + Architecture.Tests before any push
- Updated `.git/hooks/pre-push` to match committed version from `scripts/hooks/`
- Updated Aragorn and Gimli charters: added Critical Rule #1 mandating full local test run before push

**Key Learning:**
- CI must NEVER be the first place test failures are discovered
- Local validation: `dotnet test tests/Unit.Tests tests/Blazor.Tests tests/Architecture.Tests` before every push
- Razor `_Imports.razor` files: copyright header MUST come after `@using` directives if header text references any imported types

**Commit:** `100cb77` on main
**Files Modified:**
- src/Web/_Imports.razor (copyright header moved to end)
- .squad/agents/aragorn/charter.md (added mandatory full test run rule)
- .squad/agents/gimli/charter.md (added mandatory full test run rule)
### 2026-03-07: Unit.Tests Split — CI Workflow and Charter Updates (PR #95)

**Context:** Gimli split `tests/Unit.Tests` into three project-specific assemblies: `Api.Tests.Unit`, `Shared.Tests.Unit`, and `Web.Tests.Unit`.

**CI Workflow Updated (`scripts/hooks/pre-push` — Gate 3 references updated):**
- Removed references to the deleted `Unit.Tests` project
- Added `Api.Tests.Unit` and `Shared.Tests.Unit` to the test runner
- `Web.Tests.Unit` is empty placeholder — not added to gate yet

**Gimli's Charter Updated (`.squad/agents/gimli/charter.md`):**
- Replaced `Unit.Tests` references with the three new project names
- Updated Project Name Mapping table to include `Api.Tests.Unit`, `Shared.Tests.Unit`, `Web.Tests.Unit`
- Updated pre-push gate documentation to match new project structure

**Key files:**
- `.github/workflows/squad-test.yml` — updated test job matrix to run new assemblies
- `.squad/agents/gimli/charter.md` — updated project references

---

### 2026-03-06: Issue #89 — Aspire Startup Fixes VALIDATED (Sam's PR #91)

**Status:** ✅ RESOLVED — All infrastructure components validated and passing

**Current Branch:** squad/80-foundation-objectid-standardization

**Build Results:**
- AppHost: ✅ Clean (0 errors, 0 warnings)
- Api: ✅ Clean (0 errors, 0 warnings)
- Web: ✅ Clean (0 errors, 0 warnings)

**Test Results:**
- Aspire.Tests: ✅ 18/18 passing (all infrastructure orchestration tests)
- AppHost configuration validated
- ServiceDefaults configuration validated
- Auth service initialization validated
- ServiceDiscovery initialization validated

**Infrastructure Validation Complete:**
- AppHost → ServiceDefaults resource naming: ✅ Consistent
- Service registration: ✅ Correct
- Orchestration startup order: ✅ Correct
- All Aspire integration points: ✅ Passing

**Resolution:**
- Matthew's infrastructure changes (issue #89) fully integrated into Sam's PR #91 (squad/80-foundation-objectid-standardization)
- No additional infrastructure work needed
- Commented on GitHub issue #89 with validation summary
- Infrastructure scope is complete; application code scope (ObjectId refactoring) handled separately by Sam

### 2026-03-09: Test Workflow Rewrite — Split Unit Tests by Project

**Context:** squad-test.yml had test-unit bundling Api.Tests.Unit + Shared.Tests.Unit in one bash script, and stale test-aspire job referencing deleted tests/Aspire/ project.

**Changes:**
- **Split test-unit → 3 jobs:** Created separate jobs for test-api-unit (Api.Tests.Unit), test-shared-unit (Shared.Tests.Unit), test-web-unit (Web.Tests.Unit)
- **Replace test-aspire → test-apphost-unit:** New job for AppHost.Tests.Unit using dotnet test (not xUnit executable workaround). Gracefully skips if directory missing with ::notice::.
- **Job naming standardization:** All test job names now match test project names (e.g., "Api.Tests.Unit" not "Unit Tests")
- **Updated dependencies:** coverage and report jobs now depend on all 7 test jobs (test-api-unit, test-shared-unit, test-web-unit, test-architecture, test-bunit, test-integration, test-apphost-unit)
- **Artifact naming:** Each job uploads separate artifacts (api-unit-test-results, shared-unit-test-results, web-unit-test-results, apphost-unit-test-results)
- **Header comment fix:** Updated to reflect current project names (Api.Tests.Unit, Shared.Tests.Unit, Web.Tests.Unit, not old "Unit.Tests" or "Blazor.Tests")
- **squad-ci.yml integration:** Removed TODO placeholder, now calls squad-test.yml via workflow_call for DRY CI pipeline

**Key Decisions:**
- Web.Tests.Unit currently has 0 tests — job runs and succeeds (placeholder project)
- AppHost.Tests.Unit uses standard dotnet test (not xUnit executable workaround) — AppHost tests don't have the TestProcessLauncherAdapter issue
- Each unit test project gets individual job for parallelization and clearer failure reporting in CI

**Commit:** cffe0b1 on main
**Files Modified:** .github/workflows/squad-test.yml, .github/workflows/squad-ci.yml

### 2026-03-10: PR #97 Review — AppHost.Tests.Unit CI Exe Mode Fix

**Context:** Gimli fixed AppHost.Tests.Unit inconclusive tests + CI test-apphost-unit job failing due to incorrect dotnet test invocation (should use exe directly).

**PR Review Results:**
- ✅ **Workflow changes validated:** test-apphost-unit job now uses direct exe mode (matches Architecture.Tests and Web.Tests.Bunit reference patterns)
  - Build step: `dotnet build tests/AppHost.Tests.Unit --configuration Release --no-restore`
  - Run step: `cd tests/AppHost.Tests.Unit/bin/Release/net10.0 && ./AppHost.Tests.Unit -xml ... -nunit ...`
  - Pattern consistent with other xUnit v3 workaround jobs (Architecture.Tests line 258, Web.Tests.Bunit line 314)
- ✅ **Pre-push hook validated:** Added `run_apphost_tests()` function with Docker availability check
  - Docker detection: `docker info &>/dev/null 2>&1`
  - Non-blocking: Docker not required to push — tests skip gracefully if Docker unavailable via fixture's `IsAvailable` flag
  - Hook executes AppHost.Tests.Unit exe directly (lines 124-152)
- ✅ **Test infrastructure validated:**
  - `DistributedApplicationFixture.cs`: proper IAsyncLifetime with IsAvailable/UnavailableReason properties
  - `AppHostTests.cs`: uses `[Collection("AppHostIntegration")]` + `SkipException.ForSkip()` guard pattern
  - `IntegrationTestCollection.cs`: proper xUnit collection definition with `ICollectionFixture<DistributedApplicationFixture>`
  - `xunit.runner.json`: `parallelizeTestCollections: true` enabled
- ✅ **File headers validated:** All new .cs files have correct copyright headers with Project Name = "AppHost.Tests.Unit"
- ✅ **CI status:** All 25 checks passing (Test Suite, Squad CI, CodeQL, codecov)

**Key Technical Notes:**
- xUnit v3.2.2: `SkipException.ForSkip(string)` factory method required (constructor is private)
- AppHost.Tests.Unit requires direct exe execution due to xUnit v3 TestProcessLauncherAdapter compatibility issues (same as Architecture.Tests, Web.Tests.Bunit, Api.Tests.Integration)
- Shared fixture pattern reduces `DistributedApplicationTestingBuilder.CreateAsync()` calls from 5 to 1 per test run

**Merge Action:**
- PR #97 auto-merged via squash (all requirements met, branch protection satisfied)
- Branch `squad/apphost-tests-fix` will be auto-deleted on merge completion

**Commit:** (pending auto-merge completion)
**Files Modified:** .github/workflows/squad-test.yml, .git/hooks/pre-push, tests/AppHost.Tests.Unit/ (fixtures + tests)

### 2026-03-07: PR #100 Review — GitHub Actions Dependency Bumps

**Context:** Matthew Paulosky requested review and merge of Dependabot PR #100 (all CI checks passing).

**PR Summary:**
- Bumps `actions/checkout`: v4 → v6 (stable, Jan 2026)
  - Improved credential security (separate file in $RUNNER_TEMP, not .git/config)
  - Node.js 24 support
  - Runner v2.329.0+ required (satisfied by hosted runners)
- Bumps `actions/github-script`: v7 → v8 (stable)
  - Node.js runtime: v20 → v24
  - Runner v2.327.1+ required (satisfied by hosted runners)
  - All squad workflow scripts compatible (basic GitHub API calls)
- Scope: 10 workflow files updated consistently
- CI Status: All 25 checks passing (test suites, CodeQL, coverage, Architecture)

**Review Assessment:**
- ✅ Only GitHub Actions version bumps (no application code)
- ✅ Stable releases (not pre-releases or beta)
- ✅ Runner version requirements satisfied by GitHub's hosted runners
- ✅ No breaking changes to workflow logic
- ✅ All scripts compatible with Node.js 24

**Merge Action:** 
- Approved and merged via squash
- Commit: "chore(deps): Bump actions/checkout to v6 and actions/github-script to v8"
- Branch: `dependabot/github_actions/all-actions-2`

**Decision Note:** `.squad/decisions/inbox/boromir-pr100-merge.md`
