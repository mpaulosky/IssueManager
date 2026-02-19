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
