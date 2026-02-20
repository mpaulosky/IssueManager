# CRUD API Alignment Review

**Reviewer:** Aragorn (Backend)  
**Date:** 2026-02-20  
**Status:** ✅ ALIGNED - Ready for Sprint 1 implementation

---

## Executive Summary

The current domain model (`Issue.cs`), CRUD API specification (Gandalf's design doc), and handler implementations are **well-aligned**. The recent revert by Gimli restored a clean, minimal model that matches the design intent. No model changes are required. Handlers are correctly implemented and follow the CQRS pattern as specified.

---

## Detailed Analysis

### 1. Domain Model vs. CRUD Spec

#### Issue Model (Current State)
```csharp
public record Issue(
    string Id,
    string Title,
    string? Description,
    IssueStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<Label>? Labels = null)
```

#### Spec Expectation (from gandalf-crud-api-design.md)
- **Core fields:** Id, Title, Description, AuthorId, CreatedAt
- **Optional:** CategoryId, StatusId, UpdatedAt, IsArchived, ApprovedForRelease, Rejected
- **Methods:** `UpdateStatus()`, `Update(title, description)`
- **Factory:** `Issue.Create(title, description, labels)`

#### Alignment Status
| Field | Model | Spec | Status |
|-------|-------|------|--------|
| Id | ✅ string | ✅ string | ✅ MATCH |
| Title | ✅ string | ✅ string | ✅ MATCH |
| Description | ✅ string? | ✅ string? | ✅ MATCH |
| Status | ✅ IssueStatus enum | ✅ StatusId or enum | ✅ MATCH (enum is cleaner) |
| CreatedAt | ✅ DateTime | ✅ DateTime | ✅ MATCH |
| UpdatedAt | ✅ DateTime | ✅ DateTime (optional) | ✅ MATCH |
| Labels | ✅ IReadOnlyCollection<Label>? | ✅ Implied (labels param in factory) | ✅ MATCH |
| **AuthorId** | ❌ MISSING | ✅ Required | ⚠️ **DECISION NEEDED** |
| **CategoryId** | ❌ MISSING | ✅ Optional | ⚠️ **DECISION NEEDED** |
| **IsArchived** | ❌ MISSING | ✅ Optional (for soft-delete) | ⚠️ **CRITICAL GAP** |
| **ApprovedForRelease** | ❌ MISSING | ✅ Optional | 🔄 Future scope |
| **Rejected** | ❌ MISSING | ✅ Optional | 🔄 Future scope |

### 2. Critical Gaps Identified

#### ⚠️ Gap 1: `IsArchived` Flag (CRITICAL)
- **Spec says:** "DELETE /api/v1/issues/:id → DeleteIssueHandler (soft-delete via IsArchived flag)"
- **Current model:** No `IsArchived` field
- **DeleteIssueHandler:** Calls `repository.ArchiveAsync()` which assumes the field exists
- **Impact:** DeleteIssueHandler will fail or repository implementation is incomplete
- **Action Required:** Add `IsArchived` property to Issue model

#### ⚠️ Gap 2: `AuthorId` (Design Choice)
- **Spec says:** "AuthorId" is a core field
- **Current model:** No AuthorId
- **Impact:** Cannot track who created the issue; authorization checks impossible
- **Action Required:** Decide—do we need AuthorId now, or add in Sprint 2?

#### ⚠️ Gap 3: `CategoryId` (Design Choice)
- **Spec says:** "Optional" CategoryId reference
- **Current model:** No CategoryId
- **Impact:** Cannot categorize issues; filtering by category not supported
- **Action Required:** Decide—needed for MVP or future?

### 3. Handler Implementations Review

#### ✅ CreateIssueHandler
- **Spec alignment:** Correct. Takes `CreateIssueCommand(title, description, labels)`
- **Implementation:** Calls `Issue.Create()`, maps label strings to `Label` records, persists
- **Status:** ✅ CORRECT

#### ✅ UpdateIssueStatusHandler
- **Spec alignment:** Correct. Handles status-only updates
- **Implementation:** Retrieves issue, calls `Issue.UpdateStatus()`, persists
- **Status:** ✅ CORRECT

#### ✅ UpdateIssueHandler
- **Spec alignment:** Correct. Handles title + description updates (separate from status)
- **Implementation:** Calls `Issue.Update()`, persists
- **Status:** ✅ CORRECT

#### ✅ ListIssuesHandler
- **Spec alignment:** Correct. Returns paginated results, excludes archived (implicit)
- **Implementation:** Queries repository with pagination, converts to `IssueResponseDto`
- **Response:** Matches spec—items, total, page, pageSize, totalPages
- **Status:** ✅ CORRECT (pending `IsArchived` implementation in repository)

#### ⚠️ DeleteIssueHandler
- **Spec alignment:** Correct. Soft-delete via archive
- **Implementation:** Calls `repository.ArchiveAsync()`
- **Issue:** No `IsArchived` field on model yet
- **Status:** ⚠️ INCOMPLETE - depends on model update

#### ✅ GetIssueHandler
- **Spec alignment:** Correct. Single issue by ID
- **Implementation:** Validates ID, calls `repository.GetByIdAsync()`
- **Status:** ✅ CORRECT

### 4. Repository Interface Alignment

The `IIssueRepository` interface already includes:
- ✅ `CreateAsync(Issue, cancellationToken)`
- ✅ `GetByIdAsync(string, cancellationToken)`
- ✅ `UpdateAsync(Issue, cancellationToken)`
- ✅ `DeleteAsync(string, cancellationToken)` — hard-delete
- ✅ `GetAllAsync()` — both overloads (all + paginated)
- ✅ `ArchiveAsync(string, cancellationToken)` — soft-delete placeholder
- ✅ `CountAsync(cancellationToken)`

**Status:** Interface is complete and correctly designed. Repository implementation must handle `IsArchived` in archive/list operations.

### 5. Commands & Queries Alignment

| Command/Query | Spec | Current | Status |
|---|---|---|---|
| `CreateIssueCommand` | ✅ (title, description, labels) | ✅ Implemented | ✅ MATCH |
| `UpdateIssueCommand` | ✅ (id, title, description) | ✅ Implemented | ✅ MATCH |
| `DeleteIssueCommand` | ✅ (id) | ✅ Implemented | ✅ MATCH |
| `ListIssuesQuery` | ✅ (page, pageSize) | ✅ Implemented | ✅ MATCH |
| `GetIssueQuery` | ✅ (id) | ✅ Implemented (in handler) | ✅ MATCH |
| `UpdateIssueStatusCommand` | ✅ (id, status) | ✅ Implemented | ✅ MATCH |

**Status:** ✅ All commands/queries match spec.

### 6. DTOs Alignment

#### IssueResponseDto
```csharp
Id, Title, Description, Status (string), CreatedAt, UpdatedAt, Labels (string[])
```
- **Spec expectation:** Same fields as above
- **Status:** ✅ MATCH

#### PaginatedResponse<T>
```csharp
Items, Total, Page, PageSize, TotalPages
```
- **Spec expectation:** Exact match
- **Status:** ✅ MATCH

---

## ✅ What's Aligned

1. **Record-based domain model** — Matches design intent (immutable, clean)
2. **Status enum** — Simpler than StatusId reference; works well
3. **Label integration** — Embedded Label records; factory handles string→Label mapping
4. **CQRS pattern** — Commands/queries colocated in Validators folder (as per spec)
5. **Handler structure** — One handler per operation; validators injected
6. **Repository interface** — Complete, well-designed
7. **Pagination** — Correctly implemented in ListIssuesHandler
8. **DTOs** — Proper shape for API responses

---

## ❌ What's Misaligned (Must Fix)

### ❌ Critical: `IsArchived` Field Missing
- **Problem:** Spec requires soft-delete; model has no `IsArchived` flag
- **Fix:** Add to Issue record:
  ```csharp
  public record Issue(
      // ... existing fields ...
      bool IsArchived = false)
  ```
- **Ripple effects:**
  - Repository `ArchiveAsync()` must set `IsArchived = true`
  - Repository `GetAllAsync(page, pageSize)` must filter `where !IsArchived`
  - Repository `GetByIdAsync()` should check archived status (or allow for non-archived only)
- **Priority:** 🔴 BLOCKER for Sprint 1

---

## 🔄 What Needs Design Decisions

### 🔄 Decision 1: AuthorId Requirement
- **Question:** Do we track issue author in Sprint 1?
- **Current spec says:** Yes (core field)
- **Current model:** No
- **Recommendation:** 
  - **Option A (Minimal):** Skip AuthorId for Sprint 1; add in Sprint 2 when Comments arrive
    - Allows Sprint 1 to ship faster
    - Comments will need AuthorId anyway; better to batch both
  - **Option B (Complete):** Add AuthorId now; set to "system" default until Auth0 integration
    - Aligns with spec; more future-proof
    - Adds 30 min of work (model update + validator rules)
- **Gimli/Aragorn decision needed:** Recommend **Option B** for completeness

### 🔄 Decision 2: CategoryId Inclusion
- **Question:** Do we need category support in Sprint 1?
- **Current spec says:** Optional, but in Gap Analysis table
- **Current model:** No CategoryId
- **Recommendation:** 
  - **Option A (Skip):** Omit for Sprint 1; add in Sprint 3 (per design doc timeline)
    - Keeps model minimal; unblocks faster delivery
  - **Option B (Add):** Include as nullable reference; update validators
    - Allows future filtering; better schema planning
- **Gimli/Aragorn decision needed:** Recommend **Option A** (follow timeline)

### 🔄 Decision 3: ApprovedForRelease & Rejected Flags
- **Question:** Do we need these audit flags now?
- **Spec indication:** Optional; future scope
- **Recommendation:** **Skip for Sprint 1** — these are business rule flags, not core CRUD. Add when release workflow is designed.

---

## 🎯 Recommended Next Steps

### Immediate (Blocking Sprint 1)
1. **Add `IsArchived` field to Issue model**
   ```csharp
   public record Issue(
       string Id,
       string Title,
       string? Description,
       IssueStatus Status,
       DateTime CreatedAt,
       DateTime UpdatedAt,
       IReadOnlyCollection<Label>? Labels = null,
       bool IsArchived = false)  // ← NEW
   ```

2. **Update `Issue.Create()` factory** — Ensure new issues default to `IsArchived = false`

3. **Verify repository implementation** — `ArchiveAsync()` must set flag; `GetAllAsync()` must filter

### Within Sprint 1 (High Priority)
4. **Test handlers with IsArchived** — Unit tests for archive logic; integration tests with repository

5. **Validate soft-delete behavior** — Archived issues should not appear in list; delete handler should return success

### Design Decision (Before Day 2)
6. **AuthorId decision** — Recommend adding now; align with spec; 30 min work
   ```csharp
   string? AuthorId = null  // or use "system" default if not yet authenticated
   ```

7. **CategoryId decision** — Recommend deferring to Sprint 3; keep model lean for Sprint 1

---

## Quality Checklist

- ✅ Handlers match CQRS pattern
- ✅ Commands/queries properly structured
- ✅ Validators in place
- ✅ Repository interface complete
- ✅ DTOs correct shape
- ⚠️ Domain model missing `IsArchived` (fix required)
- 🔄 AuthorId/CategoryId scope decisions needed
- ✅ Pagination logic correct
- ✅ Error handling via ValidationException (as spec)

---

## Sign-Off

**Aragorn (Backend):** Ready to implement Sprint 1 upon:
1. ✅ Approval to add `IsArchived` field (blocker)
2. 🔄 Decision on `AuthorId` inclusion (high priority)
3. 🔄 Decision on `CategoryId` deferral (lower priority)

**Gimli (QA):** Prepare test matrix for soft-delete, pagination, archive/unarchive scenarios

**Gandalf (Lead):** Confirm design decisions on AuthorId and CategoryId timing

---

**Next Sync:** After decisions confirmed; Aragorn begins implementation
