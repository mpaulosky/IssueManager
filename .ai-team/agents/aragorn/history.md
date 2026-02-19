# History — Aragorn

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- C#, .NET 10.0
- Vertical Slice Architecture
- CQRS pattern
- MongoDB.EntityFramework
- ASP.NET Core APIs

**Backend patterns to follow:**
- One vertical slice per feature
- Commands handle state changes, Queries return data
- Handlers are thin orchestrators (logic in services)
- Validators ensure data integrity before command execution
- DTOs shield domain entities from the wire

**Domain modeling:**
- Issues are the core aggregate
- Think about Issue state transitions (New, Active, Resolved, Closed)
- Use domain events if needed (IssueClosed, IssueAssigned, etc.)

---

## Learnings

### Issue #2 — Shared Library Implementation (2026-02-17)

**Completed:**
- Created `IssueManager.Shared` class library (net10.0, C# 14.0)
- Registered in IssueManager.slnx solution file
- Zero external NuGet dependencies (only .NET BCL)
- Placeholder Utilities.cs class for compileability

**Patterns Established:**
- Shared library will grow organically (no premature structure)
- Content areas: DTOs, Extensions, Constants, Validators, Domain enums, Result types
- Vertical slices consume Shared selectively — no forced coupling

**Technical Details:**
- Project config matches ServiceDefaults/Api/Web conventions
- Nullable enabled, ImplicitUsings enabled
- RootNamespace: `IssueManager.Shared`
- Shared csproj uses no dependencies beyond Directory.Packages.props

---

## 2026-02-18 — Test Project Recovery (Critical Fix)

**Issue:** Missing .csproj files for 5 test projects (Unit, Architecture, BlazorTests, Integration, Aspire). Test code existed (97 test methods), but projects were unbuildable.

**Root Cause:** Test scaffolding incomplete during initial setup. Only E2E.csproj existed and built successfully.

**Resolution:**

Created all 5 missing .csproj files:

1. **Unit.csproj** — SDK: Microsoft.NET.Sdk
   - Target: net10.0
   - Dependencies: xunit, FluentAssertions, FluentValidation, NSubstitute
   - References: Shared (domain models, validators)
   - Purpose: Unit tests for domain validators, commands, queries

2. **Architecture.csproj** — SDK: Microsoft.NET.Sdk
   - Target: net10.0
   - Dependencies: xunit, FluentAssertions, NetArchTest.Rules
   - References: Shared, Api
   - Purpose: Architecture rules enforcement (layering, dependencies, naming)

3. **BlazorTests.csproj** — SDK: Microsoft.NET.Sdk.Web (required for bUnit)
   - Target: net10.0
   - Dependencies: xunit, bunit, FluentAssertions, NSubstitute
   - References: Web (components), Shared
   - Purpose: Blazor component rendering and interaction tests

4. **Integration.csproj** — SDK: Microsoft.NET.Sdk
   - Target: net10.0
   - Dependencies: xunit, FluentAssertions, Testcontainers.MongoDb, MongoDB.Driver
   - References: Shared, Api
   - Purpose: End-to-end handler tests with real MongoDB (TestContainers)

5. **Aspire.csproj** — SDK: Microsoft.NET.Sdk
   - Target: net10.0
   - Dependencies: xunit, FluentAssertions, Aspire.Hosting
   - References: AppHost, ServiceDefaults
   - Purpose: Distributed application hosting and orchestration tests

**Additional Changes:**
- Added/updated GlobalUsings.cs for each test project (common xunit, FluentAssertions imports)
- Updated IssueManager.slnx to include all 6 test projects (was missing 5)
- Upgraded MongoDB.Driver from 3.2.0 → 3.5.2 to resolve dependency conflict (MongoDB.Entities 25.0.0 requires >=3.5.2)

**Build Verification:**
- All 5 new projects build successfully
- 70 tests run and pass: Unit (30), Architecture (10), Blazor (13), Integration (17)
- Aspire tests (0) — placeholder structure ready
- E2E tests (31) exist but fail (Playwright setup issue, tracked separately)

**Key Patterns:**
- All test projects use net10.0, C# 14.0, Nullable enabled
- Consistent structure: xunit + FluentAssertions base, specialized libs per type
- Integration tests use TestContainers for MongoDB isolation
- bUnit tests require SDK.Web (not SDK) for Razor compilation

**Impact:**
- CI/CD workflow now buildable (was broken)
- Test suite can execute in full
- Gandalf's validation (I-10) unblocked
