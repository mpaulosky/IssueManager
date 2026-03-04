# Copyright Header Process — Block Format and Automation

**Date:** 2026-03-04  
**Author:** Aragorn  
**Status:** Implemented

## Context

Matthew Paulosky requested standardization of copyright headers across the IssueManager codebase to use a consistent multi-line block format instead of single-line comments.

## Decision

### Header Format

**For C# files (.cs):**
```csharp
// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     {FileName}.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  {ProjectName}
// =============================================
```

**For Razor files (.razor, .razor.cs):**
```razor
@* ============================================
   Copyright (c) 2026. All rights reserved.
   File Name :     {FileName}.razor
   Company :       mpaulosky
   Author :        Matthew Paulosky
   Solution Name : IssueManager
   Project Name :  {ProjectName}
   ============================================= *@
```

### Project Name Mapping

- `src/Api/` → `Api`
- `src/Web/` → `Web`
- `src/Shared/` → `Shared`
- `src/AppHost/` → `AppHost`
- `src/ServiceDefaults/` → `ServiceDefaults`
- `tests/Unit.Tests/` → `Unit.Tests`
- `tests/Integration.Tests/` → `Integration.Tests`
- `tests/Blazor.Tests/` → `Blazor.Tests`
- `tests/Aspire/` → `Aspire`

### Automation Mechanism

**Chosen approach: Copilot Instructions + Squad Charters (Options 3 & 4)**

#### Why not StyleCop or .editorconfig?

1. **StyleCop.Analyzers (SA1633):**
   - Adds NuGet package dependency
   - Requires build system integration
   - Limited file name templating in older .NET versions
   - Build-time overhead

2. **`.editorconfig` file_header_template:**
   - Limited support for dynamic variables (e.g., file name) in older VS versions
   - Requires IDE-specific support
   - Not as flexible for custom format

3. **Copilot Instructions (✅ CHOSEN):**
   - Zero build impact
   - Immediate effect — works NOW
   - Flexible templating
   - No package dependencies
   - Created: `.github/instructions/csharp.instructions.md`

4. **Squad Charters (✅ CHOSEN):**
   - Reinforces requirement in agent workflows
   - Updated: Aragorn charter (rule 5), Gimli charter (rule 5)
   - Ensures all squad members follow format

### Implementation

**Phase 1: Existing Files (COMPLETED)**
- Converted 4 files with single-line headers to block format
- Added headers to 18 files with no copyright
- Total: 22 files standardized

**Phase 2: Automation (COMPLETED)**
- Created `.github/instructions/csharp.instructions.md` with full template and project mapping
- Updated Aragorn and Gimli charters to include block format requirement
- Agents will now add headers automatically when creating new files

**Verification:**
- Web project builds: ✅ SUCCESS (0 errors, 0 warnings)
- Pre-push tests: ✅ PASSED
- Commits pushed to main: cb6f9bf (tests), 91eee02 (src+automation)

## Rationale

1. **Consistency:** All files will have identical header format with metadata
2. **Traceability:** File name, project name, author documented at file level
3. **Zero Build Cost:** No analyzer packages or build system changes
4. **Immediate Effect:** Works for Copilot CLI and squad agents right away
5. **Maintainability:** Single source of truth in `.github/instructions/` for format

## Alternatives Considered

- **StyleCop SA1633:** Too heavyweight, adds build-time cost
- **`.editorconfig`:** Limited templating, IDE-dependent
- **Manual enforcement:** Not scalable

## Consequences

### Positive
- All new files will have consistent headers automatically
- No build performance impact
- No new package dependencies
- Easy to update format centrally if needed

### Negative
- Relies on Copilot/agent compliance (no build-time enforcement)
- Existing files in tests/ not yet updated (can be done incrementally)

## Follow-Up

- ✅ Update tests/ directory headers (optional, can be done in future cleanup)
- ✅ Document in squad wiki if format changes in future

## References

- Implementation: Commits cb6f9bf, 91eee02
- Instruction file: `.github/instructions/csharp.instructions.md`
- Charter updates: `.squad/agents/aragorn/charter.md`, `.squad/agents/gimli/charter.md`
