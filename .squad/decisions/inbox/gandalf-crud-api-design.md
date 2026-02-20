# 2026-02-19: CRUD API Design for Shared Models

**By:** Gandalf (Lead)

**What:** Comprehensive CRUD API structure for Issue, Comment, User, Category, and Status models using CQRS command/query pattern with vertical slice organization.

**Why:** Current API project has only partial endpoints (Create, GetById, UpdateStatus). Full CRUD coverage ensures consistency, enables team parallelization (Aragorn, Gimli, Arwen can work independently on features), and establishes clear patterns for future vertical slices.

---

## Current State Analysis

### Models in Shared Project

1. **Issue** (src/Shared/Domain/Models/Issue.cs)
   - Aggregate root with builder pattern
   - Core fields: Id, Title, Description, AuthorId, CreatedAt
   - Optional: CategoryId, StatusId, UpdatedAt, IsArchived, ApprovedForRelease, Rejected
   - Tracks audit metadata (timestamps, flags)
   - Factory: `Issue.Create(title, description, labels)`
   - Methods: `UpdateStatus()`, `Update(title, description)`

2. **Comment** (src/Shared/Domain/Models/Comment.cs)
   - Record type: Id, Title, Description, IssueId, AuthorId, CreatedAt
   - Optional: UserVotes (HashSet<string>), IsAnswer flag, AnswerSelectedById
   - Embeds voting logic and answer selection

3. **User** (src/Shared/Domain/Models/User.cs)
   - Record type: Id, Name, Email
   - Minimal—read-only (no archive flag)
   - No update tracking

4. **Category** (src/Shared/Domain/Models/Category.cs)
   - Record type: Id, Name, Description
   - Reference data (lookup table pattern)
   - No audit timestamps

5. **Status** (src/Shared/Domain/Models/Status.cs)
   - Record type: Id, Name, Description
   - Reference data (lookup table pattern)
   - No audit timestamps

### Existing API Patterns

- **Repository pattern:** `IIssueRepository` (CRUD ops) in src/Api/Data/
- **Command/Query models:** `CreateIssueCommand` in src/Shared/Validators/
- **Handlers:** `CreateIssueHandler`, `GetIssueHandler`, `UpdateIssueStatusHandler` in src/Api/Handlers/
- **Validators:** FluentValidation via `CreateIssueValidator`
- **Wire-up:** Program.cs uses `builder.Services.AddOpenApi()` (Scalar-based)

### Gap Analysis

| Model    | Create | Read  | Update | Delete | Archive |
|----------|--------|-------|--------|--------|---------|
| Issue    | ✓ (partial)     | ✓ (ById only)    | ✓ (StatusOnly)     | ✗      | ✓ (flag exists) |
| Comment  | ✗      | ✗     | ✗      | ✗      | ✗       |
| User     | ✗      | ✗     | ✗      | ✗      | N/A     |
| Category | ✗      | ✗     | ✗      | ✗      | N/A     |
| Status   | ✗      | ✗     | ✗      | ✗      | N/A     |

---

## Recommended API Design

### RESTful Endpoints

All endpoints follow REST conventions with CQRS command/query handlers behind the scenes.

```
POST   /api/v1/issues                    → CreateIssueHandler
GET    /api/v1/issues/:id                → GetIssueHandler
PATCH  /api/v1/issues/:id                → UpdateIssueHandler
DELETE /api/v1/issues/:id                → DeleteIssueHandler (soft-delete via IsArchived flag)
GET    /api/v1/issues                    → ListIssuesHandler (paginated)

POST   /api/v1/issues/:issueId/comments  → CreateCommentHandler
GET    /api/v1/issues/:issueId/comments  → ListCommentsHandler (paginated)
GET    /api/v1/comments/:commentId       → GetCommentHandler
PATCH  /api/v1/comments/:commentId       → UpdateCommentHandler (vote, answer selection)
DELETE /api/v1/comments/:commentId       → DeleteCommentHandler

POST   /api/v1/users                     → CreateUserHandler
GET    /api/v1/users/:id                 → GetUserHandler
PATCH  /api/v1/users/:id                 → UpdateUserHandler
DELETE /api/v1/users/:id                 → DeleteUserHandler (hard delete—no archive flag)
GET    /api/v1/users                     → ListUsersHandler (paginated)

POST   /api/v1/categories                → CreateCategoryHandler
GET    /api/v1/categories/:id            → GetCategoryHandler
PATCH  /api/v1/categories/:id            → UpdateCategoryHandler
DELETE /api/v1/categories/:id            → DeleteCategoryHandler (hard delete—reference data)
GET    /api/v1/categories                → ListCategoriesHandler

POST   /api/v1/statuses                  → CreateStatusHandler
GET    /api/v1/statuses/:id              → GetStatusHandler
PATCH  /api/v1/statuses/:id              → UpdateStatusHandler
DELETE /api/v1/statuses/:id              → DeleteStatusHandler (hard delete—reference data)
GET    /api/v1/statuses                  → ListStatusesHandler
```

### Command/Query Structure

Commands and Queries go in **src/Shared/Validators/** (colocated with validators):

```csharp
// Commands
public record CreateIssueCommand(string Title, string? Description, List<string>? Labels);
public record UpdateIssueCommand(string Id, string Title, string? Description);
public record DeleteIssueCommand(string Id);
public record ArchiveIssueCommand(string Id);

public record CreateCommentCommand(string IssueId, string Title, string Description);
public record UpdateCommentCommand(string Id, string Title, string Description);
public record DeleteCommentCommand(string Id);

// Queries
public record GetIssueQuery(string Id);
public record ListIssuesQuery(int Page = 1, int PageSize = 20);
public record ListCommentsQuery(string IssueId, int Page = 1, int PageSize = 20);
```

### Handlers Location

- **src/Api/Handlers/** — All CQRS handlers (Create, Update, Get, List, Delete)
  - One handler per command/query
  - Naming: `{Action}{Model}Handler` (e.g., `CreateIssueHandler`, `ListCommentsHandler`)

### Request/Response Shapes

**Create Issue**
```json
POST /api/v1/issues
{
  "title": "Login page broken",
  "description": "Users cannot log in after password reset",
  "labels": ["bug", "high-priority"]
}

Response 201:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Login page broken",
  "description": "...",
  "status": "Open",
  "createdAt": "2026-02-19T10:30:00Z",
  "updatedAt": "2026-02-19T10:30:00Z",
  "isArchived": false,
  "labels": ["bug", "high-priority"]
}
```

**List Issues (Paginated)**
```json
GET /api/v1/issues?page=1&pageSize=20

Response 200:
{
  "items": [
    { "id": "...", "title": "...", ... }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

**Update Issue**
```json
PATCH /api/v1/issues/550e8400-e29b-41d4-a716-446655440000
{
  "title": "Login page broken - WIP",
  "description": "Users cannot log in. Backend issue found."
}

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "updatedAt": "2026-02-19T11:00:00Z",
  ...
}
```

**Delete Issue (Soft-delete)**
```json
DELETE /api/v1/issues/550e8400-e29b-41d4-a716-446655440000

Response 200 (no body) or 204 No Content
```

---

## Cross-Cutting Concerns

### 1. Authentication & Authorization

- **All Issue/Comment endpoints:** Require authenticated user (bearer token or session)
- **User context:** Extract from JWT claim `sub` (subject) → authorize user operations
  - Users can only update/delete their own comments
  - Only issue author or admin can update/delete issue
  - Category/Status are admin-only (CRUD)
- **Admin role:** Required for User, Category, Status CRUD
- **Implementation:** Add authorization middleware in Program.cs; decorate handlers with `[Authorize]` and `[Authorize(Roles = "Admin")]`

### 2. Validation

- **FluentValidation:** Extend existing pattern
  - `CreateIssueValidator` ✓ (exists)
  - `CreateCommentValidator` (new)
  - `UpdateCommentValidator` (new)
  - etc.
- **Consistency:** All non-empty strings must pass `NotEmpty().MaxLength(x)` rules
- **Cross-field validation:** No updates to archived issues (validate in handler)

### 3. Error Handling

- **Standard error response:**
  ```json
  {
    "type": "https://api.example.com/errors/validation-failed",
    "title": "Validation Failed",
    "status": 400,
    "detail": "...",
    "errors": { "title": ["Title is required"] }
  }
  ```
- **Codes:**
  - 400: Validation failure
  - 401: Unauthenticated
  - 403: Unauthorized
  - 404: Not found
  - 409: Conflict (e.g., archive non-existent issue)
  - 500: Server error

### 4. Pagination

- **List endpoints:** All return paginated results
  - Query params: `page` (1-indexed), `pageSize` (default 20, max 100)
  - Response includes: `items[]`, `total`, `page`, `pageSize`, `totalPages`
- **Filtering:** Future expansion (not in scope this iteration)

### 5. Soft-Delete (Archive)

- **Issue.IsArchived flag:** Used for soft-delete
  - `DELETE /api/v1/issues/:id` sets `IsArchived = true` instead of removing row
  - List/Get queries exclude archived issues by default
  - Future: Support `?includeArchived=true` query param for admin view
- **Comments:** Hard-delete (no archive flag—related to issue anyway)
- **Reference data (Category, Status, User):** Hard-delete (no archive needed)

---

## Risk Assessment

### Risk 1: Cascade Delete on Issue → Comments

**Problem:** Deleting an issue should cascade-delete comments (or soft-delete if archived).
**Mitigation:**
- Define policy: If issue is archived, comments inherit archived state
- Repository layer handles cascade (MongoDB transaction or manual cleanup)
- Document in Comment model: "Comments are dependent on Issue; deleting Issue deletes Comments"

### Risk 2: Update Conflict on Nested Resources

**Problem:** Users might concurrently update same issue—last-write-wins or conflict?
**Mitigation:**
- Start simple: Last-write-wins (overwrite UpdatedAt)
- Future: Add `version` field (optimistic locking) if needed
- Document in API: "Concurrent updates not supported; use polling for real-time sync"

### Risk 3: Archive State Coherence

**Problem:** IsArchived flag on Issue; how does Comment visibility work if parent is archived?
**Mitigation:**
- Handler logic: Archive issue → all comments filtered out of List responses
- Document: "Archived issues and their comments are invisible to non-admin users"

### Risk 4: User Model Write Implications

**Problem:** User model is minimal (Id, Name, Email). Can users update their own profile?
**Mitigation:**
- Decision: Yes, users can update Name and Email
- Add fields if needed (phone, avatar, preferences) during implementation
- Restrict to authenticated user (authorization check in handler)

### Risk 5: Breaking Changes if Refactoring Existing Endpoints

**Problem:** Current handlers exist; new ones might contradict old patterns.
**Mitigation:**
- Keep existing handlers (CreateIssueHandler, GetIssueHandler, UpdateIssueStatusHandler)
- Rename or extend: `UpdateIssueStatusHandler` → `UpdateIssueStatusCommand`; new `UpdateIssueHandler` for title/description
- Deprecate separately if needed (version API)
- Run tests continuously (CI catches breaking changes)

---

## Decomposition for Implementation

### Sprint 1: Issue CRUD Foundation

**Owner:** Aragorn (Backend)

1. **Issue Repository Layer** (2h)
   - Extend `IIssueRepository`: Add `GetAllAsync(pagination)`, `ArchiveAsync(id)`
   - Implement in MongoDB (MongoDB.EntityFramework)

2. **Commands & Queries** (1h)
   - Create: `UpdateIssueCommand`, `DeleteIssueCommand`, `ListIssuesQuery`, `ArchiveIssueCommand`
   - Colocate in src/Shared/Validators/

3. **Handlers** (3h)
   - `UpdateIssueHandler` (Patch title + description)
   - `DeleteIssueHandler` (Soft-delete via IsArchived flag)
   - `ListIssuesHandler` (Paginated, excludes archived)

4. **Validators** (1h)
   - `UpdateIssueValidator`
   - `ArchiveIssueValidator` (simple—just id)

5. **API Endpoints** (1.5h)
   - Wire handlers in Program.cs
   - Map GET, PATCH, DELETE, GET list routes

**Testing Owner:** Gimli (Tester)

- Unit tests: Handler logic, validator rules (80% coverage min)
- Integration tests: Repository layer with TestContainers MongoDB (80% coverage min)
- Validation edge cases: Empty title, max length, pagination bounds

### Sprint 2: Comment CRUD

**Owner:** Aragorn (Backend)

1. **Comment Repository** (1.5h)
2. **Commands & Queries** (1h)
3. **Handlers** (2.5h) — including vote/answer logic
4. **Validators** (1h)
5. **API Endpoints** (1h)

**Testing:** Gimli (80% handler + repository coverage)

### Sprint 3: User, Category, Status CRUD

**Owner:** Aragorn (Backend)

1. **Repositories** for User, Category, Status (2h)
2. **Commands, Queries, Handlers** (3h per model)
3. **Validators** (1h)
4. **API Endpoints** (2h)
5. **Authorization** (1h) — Admin-only endpoints

**Testing:** Gimli (80% coverage); Legolas (authorization in CI)

### Frontend Integration (Parallel)

**Owner:** Arwen (Frontend)

- Create Blazor forms/components for Issue CRUD after Aragorn completes Sprint 1 endpoint definitions
- E2E via Playwright (when endpoints are stable)

---

## Key Recommendations

### 1. **Soft-Delete Pattern for Audit Domains**

Use `IsArchived` flag (not hard-delete) for Issues and Comments. This preserves audit trail, enables undo, and supports compliance. Hard-delete for reference data (Category, Status, User) because they're lookups, not audit-tracked.

### 2. **CQRS Command/Query Cohabitation in Validators Folder**

Commands and Queries belong in `src/Shared/Validators/` alongside validators. This mirrors existing pattern (`CreateIssueCommand` is already there) and keeps domain contracts colocated with validation rules. Reduces cognitive load during feature development.

### 3. **Pagination First, Filtering Later**

All List endpoints must support pagination from day one (page, pageSize, totalPages in response). Filtering by status, labels, date range can be added incrementally. This prevents data explosion as issue count grows and trains Gimli/Aragorn on consistent pattern.

---

## Next Steps

1. **Aragorn** kickstarts Sprint 1 (Issue CRUD) after this decision is approved
2. **Gimli** prepares test matrix (unit, integration, boundary cases) in parallel
3. **Arwen** awaits Issue CRUD endpoints (Sprint 1 completion) to begin UI
4. **Gandalf** schedules weekly architecture sync to catch integration friction
5. **Legolas** prepares MongoDB test containers and CI updates (MongoDB service container already in test.yml ✓)

---

**Decision Status:** Awaiting team consensus (Gimli quality review, Aragorn feasibility check)
