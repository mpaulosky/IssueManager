# PR #52 Review — Phase 1 Test Compilation Fixes (Issue #51)

**Status:** ❌ REJECTED  
**Reviewer:** Aragorn (Lead Developer)  
**Date:** 2026-02-24  
**PR:** #52 — test: Phase 1 test compilation fixes - Issue #51  

---

## Summary

**Phase 1 verification is correct and complete**, but **scope creep in Directory.Packages.props violates architectural discipline**. The PR mixes verification documentation with unrelated infrastructure changes.

---

## What Works ✅

1. **Gimli's Phase 1 Verification:** Accurate and thorough
   - Entity constructor parameters correctly use DTO objects (UserDto, CategoryDto, StatusDto)
   - Property naming correct (.Archived, not .IsArchived)
   - IssueRepository API contracts properly aligned
   - All test projects compile without errors

2. **Documentation Quality:** Clear, detailed findings in `gimli-issue-51-phase1-findings.md`

3. **CI/CD Status:** All 10 workflows passing, no compilation errors

---

## What Fails ❌

### Issue: Scope Creep in Directory.Packages.props

**Lines changed:**
```xml
<PackageVersion Include="Aspire.Hosting.Aspire" Version="13.1.1" />
<PackageVersion Include="Aspire.Hosting.Redis" Version="13.1.1" />
```

**Problem:**
1. **Out of scope:** Issue #51 is "Test Compilation Failures: Domain Model API Changes" — package versioning is unrelated
2. **Unused:** No grep results for these packages in `/src` or `/tests`
3. **No justification:** PR description doesn't explain why these are needed
4. **Violates vertical slice architecture:** Different concerns (test fixes + infra changes) mixed in one PR
5. **Requires separate justification:** Any infrastructure changes need their own issue and documented rationale

---

## Architectural Principle Violated

**From custom instructions:**
> Enforce Dependency Injection: true
> Centralize NuGet Package Versions: true (all package versions must be managed in Directory.Packages.props; **document the reason**)

The package additions are centralized correctly but **lack documented justification**. Package version changes need explicit scoping.

---

## Recommendation

**Route back to Gimli for revision:**

1. **Remove package changes** from Directory.Packages.props (revert to pre-PR state)
2. **Keep documentation** (gimli-issue-51-phase1-findings.md and history updates)
3. **If packages are needed:** Create separate issue #52b (or new issue) with:
   - Rationale for Aspire.Hosting.Aspire and Redis integration
   - Which projects consume them
   - Version alignment with .NET 10 requirements
   - Separate PR for infrastructure changes

---

## Files Affected

- ✅ `.squad/agents/gimli/history.md` — GOOD (Phase 1 documentation)
- ✅ `.squad/decisions/inbox/gimli-issue-51-phase1-findings.md` — GOOD (verification detail)
- ❌ `Directory.Packages.props` — REJECT (scope creep)

---

## Phase 1 is Actually Complete

The work Gimli did is correct: **there are no Phase 1 compilation errors to fix**. The tests already align with the domain model. The PR should document this fact and close the issue, but without infrastructure changes mixed in.

---

## Next Steps

1. **Gimli:** Revise PR #52 by removing Directory.Packages.props changes
2. **Gimli:** Re-push cleaned branch
3. **Aragorn:** Re-review and approve documentation-only PR
4. **If packages needed:** Create new issue with rationale and assign separately

---

## Decision Authority

As Lead Developer (Aragorn), I enforce this architectural gate because:
- PR scope must be single-concern (test fixes OR infrastructure, not both)
- Infrastructure changes require documented justification
- Vertical slice principle must hold: separate concerns → separate PRs
