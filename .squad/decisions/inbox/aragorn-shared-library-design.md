# Decision — Shared Library Structure

**Date:** 2026-02-17  
**By:** Aragorn  
**Decision:** Design of the IssueManager.Shared library

## Context

Issue #2 required creation of a new `Shared` class library to serve as a common dependency across vertical slices. The library needed to follow the same conventions as existing projects (ServiceDefaults, Api, Web).

## Decision

### Project Structure

**Location:** `src/Shared/`  
**Name:** `IssueManager.Shared`  
**Target Framework:** `.NET 10.0`  
**Language Version:** `C# 14.0`

### Configuration

The Shared project uses standard .NET conventions:
- `Nullable` enabled for null-safety
- `ImplicitUsings` enabled for cleaner code
- Root namespace: `IssueManager.Shared`
- No external NuGet dependencies (uses only .NET BCL)

### Content Strategy

The library begins with a placeholder `Utilities.cs` class. As features are built, the Shared library will contain:

1. **Common DTOs** — Shared request/response types across vertical slices
2. **Extensions** — Extension methods for common operations
3. **Constants** — Application-wide configuration constants
4. **Validation Rules** — Reusable FluentValidation rules
5. **Domain Types** — Shared enums (e.g., IssueStatus, IssuePriority)
6. **Result Types** — Standard Result<T> or OneOf<T> for consistent error handling

### Dependency Management

The Shared library has **zero external dependencies**. It depends only on:
- .NET 10.0 runtime
- C# 14.0 language features

This keeps the library lightweight and ensures no dependency bloat for consuming projects.

### No Structure Enforcement Yet

Subdirectories (Extensions/, Validators/, Models/, Constants/) can be created as needed per feature. Premature structure causes friction; we'll organize organically as code grows.

## Rationale

1. **Vertical Slice Safety** — Shared library does NOT enforce cross-slice dependencies. Slices use only what they need.
2. **No Bloat** — Zero external dependencies makes Shared fast and reliable.
3. **Growth Path** — Structure will emerge naturally as patterns appear.
4. **Convention Consistency** — Uses same csproj conventions as ServiceDefaults, Api, and Web.

## Status

✅ **Accepted**
