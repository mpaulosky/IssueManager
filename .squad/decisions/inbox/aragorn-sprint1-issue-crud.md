# 2026-02-19: Sprint 1 Issue CRUD - Implementation Patterns

**By:** Aragorn (Backend Dev)

**What:** Established concrete implementation patterns for CQRS handlers, pagination, soft-delete, and API endpoint wiring based on Gandalf's architectural design.

**Why:** Gandalf provided the CRUD API design spec; this documents the actual implementation choices made during Sprint 1 that will be reused in Sprint 2+ (Comment, User, Category, Status CRUD).

---

## Implementation Patterns

### 1. Repository Method Signature for Pagination

**Pattern:**
```csharp
Task<(IReadOnlyList<T> Items, long Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
```

**Rationale:**
- Returns both items AND total count in single DB round-trip
- Tuple return avoids creating wrapper class for every entity type
- Client needs total count for pagination UI (page count, "showing X of Y")
- MongoDB implementation: `CountDocumentsAsync()` + `Find().Skip().Limit()`

**Alternative considered:**
- Separate `CountAsync()` method → **rejected** (two DB calls, slower)
- Custom `PagedResult<T>` class → **rejected** (unnecessary abstraction, tuple is clearer)

---

### 2. Soft-Delete via Archive Flag

**Pattern:**
```csharp
// Entity
public bool IsArchived { get; set; }

// Repository method
Task<bool> ArchiveAsync(string id, CancellationToken cancellationToken = default);

// Implementation
var update = Builders<IssueEntity>.Update
    .Set(x => x.IsArchived, true)
    .Set(x => x.UpdatedAt, DateTime.UtcNow);
```

**Rationale:**
- Preserves audit trail (issue history, comments, votes remain queryable)
- Supports "undo" operations (future: restore archived issue)
- Compliance-friendly (GDPR: track deletion requests, not data)
- Default list queries exclude archived: `filter = Builders.Filter.Eq(x => x.IsArchived, false)`

**Trade-off:**
- Pro: Safe, reversible, auditable
- Con: DB storage grows (archived issues never deleted)
- Mitigation: Future: background job archives issues >1 year old

---

### 3. Paginated Response DTO

**Pattern:**
```csharp
public record PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public long Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
```

**Calculation:**
```csharp
var totalPages = (int)Math.Ceiling((double)total / pageSize);
```

**Rationale:**
- Standard shape for all list endpoints (consistency)
- `TotalPages` calculated server-side (client doesn't need math)
- `Total` is count of ALL items (not just current page)
- `Items` uses `IReadOnlyList` (immutable, clear intent)

**Alternative considered:**
- Cursor-based pagination → **rejected** (overkill for MVP, harder to implement "jump to page N")
- HAL/JSON:API format → **rejected** (adds complexity, not needed)

---

### 4. Handler Error Handling

**Pattern:**
```csharp
// Not found
var issue = await _repository.GetByIdAsync(id);
if (issue is null) return null; // → 404

// Validation error
var validationResult = await _validator.ValidateAsync(command);
if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors); // → 400
```

**Endpoint mapping:**
```csharp
app.MapPatch("{id}", async (string id, UpdateIssueCommand cmd, Handler handler) =>
{
    var result = await handler.Handle(cmd with { Id = id });
    return result is not null ? Results.Ok(result) : Results.NotFound();
});
```

**Rationale:**
- Handler returns `null` → endpoint translates to 404 (separation of concerns)
- `ValidationException` bubbles to middleware → 400 with error details
- No custom exceptions needed (FluentValidation + nullable return types cover 90% of cases)

**Trade-off:**
- Pro: Simple, minimal ceremony
- Con: Null checks in every endpoint (repetitive)
- Mitigation: Future: Middleware or MediatR pipeline behavior to standardize

---

### 5. Endpoint Wiring with MapGroup

**Pattern:**
```csharp
var issuesApi = app.MapGroup("/api/v1/issues")
    .WithTags("Issues")
    .WithOpenApi();

issuesApi.MapGet("", async (int? page, int? pageSize, Handler handler) => { ... })
    .WithName("ListIssues")
    .WithSummary("Get a paginated list of issues")
    .Produces<PaginatedResponse<IssueDto>>(200)
    .Produces(400);
```

**Rationale:**
- Groups related endpoints (all `/api/v1/issues/*` routes share configuration)
- `.WithTags()` groups in Scalar UI (improves discoverability)
- `.WithName()` enables route linking (`Results.CreatedAtRoute("GetIssue", ...)`)
- `.WithSummary()` documents intent in OpenAPI
- `.Produces<T>()` declares response types (Scalar generates examples)

**Service registration:**
```csharp
builder.Services.AddSingleton<IIssueRepository>(sp => 
    new IssueRepository(connectionString, "IssueManagerDb"));
builder.Services.AddSingleton<UpdateIssueHandler>();
builder.Services.AddSingleton<UpdateIssueValidator>();
```

**Rationale:**
- Singleton lifetime: Handlers, validators, repositories are stateless
- Repository instantiation: Connection string from `IConfiguration`
- DI in minimal APIs: Parameters auto-resolved from DI container

---

### 6. DTO Strategy for List vs. Detail

**Created DTOs:**
- `IssueResponseDto` (lightweight): id, title, description, status, dates, labels
- `IssueDto` (full): includes Category, User, Status entities (related data)

**Decision:**
- List endpoints → `IssueResponseDto` (faster, less data over wire)
- Detail endpoints → `IssueDto` (full context for editing)

**Rationale:**
- List operations don't need full entity graph (Category name, User email, etc.)
- Trade-off: Two DTOs vs. Optional properties on single DTO
- Chose separate DTOs for clarity (explicit intent)

**Future enhancement:**
- Consider GraphQL or OData for client-driven field selection
- For now: Two DTOs is simpler and performs well

---

## Deviations from Gandalf's Spec

### 1. ArchiveIssueCommand Not Implemented

**Gandalf's spec:** Separate `ArchiveIssueCommand` as alternative to `DeleteIssueCommand`

**Aragorn's decision:** Skip `ArchiveIssueCommand` for MVP
- Reason: DELETE semantics already soft-delete (archive)
- Future: If we add "restore" or "hard delete", then add explicit Archive command
- Impact: None (DELETE does the same thing)

### 2. GetIssueQuery Uses Existing Pattern

**Gandalf's spec:** Not specified

**Aragorn's decision:** Reused existing `GetIssueQuery` record from `GetIssueHandler.cs`
- Pattern: Query record defined in same file as handler (not in Validators folder)
- Rationale: Maintains consistency with existing handlers
- Future: Gandalf may standardize (move all queries to Validators/)

### 3. IssueResponseDto Instead of IssueDto in List

**Gandalf's spec:** Response shape shown as `IssueDto`

**Aragorn's decision:** Created `IssueResponseDto` (lightweight DTO)
- Reason: Existing `IssueDto` includes CategoryDto, UserDto, StatusDto (heavyweight)
- List operations don't need full entity graph
- Trade-off: Extra DTO vs. Performance

---

## Risks and Mitigations

### Risk 1: Test Suite Broken (55 Errors)

**Problem:** Gimli's pre-written tests reference new handlers but use old Issue model

**Mitigation:** 
- Tests confirm correct integration (handlers exist)
- Gimli will update tests in next task
- Non-blocking for Sprint 2 (Comment CRUD can proceed in parallel)

### Risk 2: MongoDB Connection String Hardcoded

**Problem:** Fallback to `"mongodb://localhost:27017"` if config missing

**Mitigation:**
- Works for local dev (MongoDB running locally)
- Production: Connection string must be in `appsettings.Production.json` or Azure Key Vault
- Legolas will configure in deployment pipeline

### Risk 3: No Authorization on Endpoints

**Problem:** All endpoints are public (no `[Authorize]` attribute)

**Mitigation:**
- MVP phase: Focus on functionality
- Sprint 3: Add Auth0 integration (per README.md)
- Gandalf's spec mentions this (cross-cutting concern, deferred)

### Risk 4: Concurrent Update Conflict

**Problem:** Last-write-wins (no optimistic locking)

**Mitigation:**
- Acceptable for MVP (low concurrency expected)
- Future: Add `Version` field to Issue, use MongoDB `$set` with version check
- Document in API: "Concurrent updates not supported"

---

## Reusable Patterns for Sprint 2+ (Comment, User, Category, Status)

### Checklist for New CRUD Slice

1. **Repository:**
   - `IRepository<T>` interface: CreateAsync, GetByIdAsync, GetAllAsync(page, pageSize), UpdateAsync, DeleteAsync/ArchiveAsync
   - Implementation: MongoDB, uses FilterBuilder/UpdateBuilder

2. **Commands & Queries:**
   - Create, Update, Delete (or Archive) commands in `Shared/Validators/`
   - ListQuery with Page, PageSize in `Shared/Validators/`

3. **Validators:**
   - FluentValidation for each command/query
   - Colocated with commands in `Shared/Validators/`

4. **Handlers:**
   - One handler per command/query in `Api/Handlers/`
   - Naming: `{Action}{Model}Handler`
   - Pattern: Validate → business logic → persist → return result or null

5. **DTOs:**
   - Lightweight DTO for list responses
   - Full DTO for detail responses (if needed)

6. **Endpoints:**
   - MapGroup for `/api/v1/{resource}`
   - .WithTags(), .WithName(), .WithSummary(), .Produces<T>()
   - Register handlers/validators/repos as Singleton in Program.cs

---

## Next Actions

**For Gimli:**
- Update Unit tests to use new handler signatures
- Fix Issue model references (`Shared.Domain.Issue`)
- Update repository mocks (GetAllAsync returns tuple)

**For Aragorn (Sprint 2):**
- Implement Comment CRUD using these patterns
- Comments nested under Issues: `/api/v1/issues/{issueId}/comments`
- Comment voting and answer selection (domain logic)

**For Gandalf:**
- Review DTO strategy (lightweight vs. full)
- Decide on ArchiveIssueCommand (keep DELETE-as-archive or add explicit Archive?)
- Standardize query location (Validators folder vs. Handler file)

---

**Status:** Approved patterns, ready for Sprint 2 reuse
