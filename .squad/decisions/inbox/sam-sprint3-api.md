# Sam Sprint 3 API Hardening Decisions

**Date:** 2026-02-27
**Agent:** Sam (Backend Developer)
**Branch:** feat/sprint-3-hardening

## Decisions

### 1. Scalar API Reference UI Configuration
**What:** Added `app.MapScalarApiReference()` in Program.cs after `app.MapOpenApi()`
**Why:** Provides interactive API documentation UI for developers. Scalar is the project's standard (per copilot-instructions.md), not Swagger UI
**Implementation:** Required adding `global using Scalar.AspNetCore;` to GlobalUsings.cs since ImplicitUsings doesn't include it
**Impact:** API documentation now accessible at `/scalar/v1` endpoint

### 2. CORS Policy Configuration
**What:** Added CORS policy with configurable origins from `Cors:AllowedOrigins` config section
**Why:** Enables cross-origin requests from Web frontend to Api backend
**Implementation:**
- Added in Program.cs before `builder.Build()`
- Defaults to `["https://localhost:7001", "http://localhost:5001"]` if not configured
- Uses `AddDefaultPolicy` with `AllowAnyHeader()` and `AllowAnyMethod()`
- Called `app.UseCors()` after `UseHttpsRedirection()`
**Impact:** Frontend can now call API endpoints without CORS errors

### 3. Nullable Reference Warning Resolution Pattern
**What:** Fixed 14 CS8603/CS8625 warnings using null-forgiving operator `!` on `Result.Value`
**Why:** Result<T> pattern guarantees Value is non-null when Success is true, but compiler can't infer this
**Pattern Established:**
```csharp
var result = await _repository.SomeAsync(...);
if (result.Failure)
    throw new NotFoundException(...);
return result.Value!; // Safe: Failure check guarantees Value is non-null
```
**Alternative Considered:** Could use `result.Success ? result.Value : throw ...` but the pattern above is more readable and matches existing handlers
**Files Changed:** CreateIssueHandler, GetIssueHandler, CreateStatusHandler, UpdateStatusHandler, CreateCommentHandler, UpdateCommentHandler, CreateCategoryHandler, UpdateCategoryHandler
**Impact:** Clean build with 0 nullable reference warnings in handlers

### 4. Comment Filtering by Issue ID
**What:** Extended GET /api/v1/comments to accept optional `?issueId=` query parameter
**Why:** Comment detail page needs to display only comments for a specific issue
**Implementation:**
- Updated `ICommentRepository.GetAllAsync(string? issueId = null, CancellationToken cancellationToken = default)`
- CommentRepository uses `Builders<Comment>.Filter.Eq(c => c.Issue.Id, objectId)` when issueId provided
- ListCommentsHandler passes issueId to repository
- CommentEndpoints accepts `string? issueId` parameter from query string
- ICommentApiClient and CommentApiClient updated to accept optional issueId and append as query param
**MongoDB Filter Pattern:** Start with `Filter.Empty`, apply conditional `Eq()` filter when issueId is valid ObjectId
**Impact:** API now supports filtering comments by issue, enabling Legolas to build issue detail page with filtered comments

### 5. Global Using for Third-Party Packages
**What:** Added `global using Scalar.AspNetCore;` to GlobalUsings.cs
**Why:** ImplicitUsings only covers BCL and ASP.NET Core namespaces, not third-party packages like Scalar
**Pattern:** When adding new packages that are used across multiple files, add global using to GlobalUsings.cs
**Impact:** Keeps Program.cs clean without explicit using directives

## Testing Notes
- Api.csproj builds clean (0 errors, 0 warnings)
- Web.csproj has errors in IssueDetailPage.razor but those are Legolas's responsibility (UI layer)
- All changes maintain backward compatibility (optional parameters)
- No breaking changes to existing API contracts
