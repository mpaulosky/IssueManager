# Sam — History

## Core Context
Backend Developer on IssueManager (.NET 10, MongoDB, EF Core, CQRS, MediatR, Minimal APIs). User: Matthew Paulosky.

## Learnings

### Repository patterns
- `IssueRepository`: works with `IssueDto` directly
- `CategoryRepository`, `StatusRepository`, `CommentRepository`: use domain `Model` types with `Result<T>` return
- `Shared.Abstractions.Result` is the standard return type for repository operations

### API endpoint patterns
- Endpoints registered via `IEndpointRouteBuilder` extension methods
- `Program.cs` calls `app.MapEndpoints()`
- `public partial class Program {}` required at bottom of Program.cs for test factory access
- DI registration in `ServiceCollectionExtensions` methods, not in Program.cs directly

### Key file paths
- API: `src/Api/`
- Repositories: `src/Api/Data/` (IIssueRepository, IssueRepository)
- Endpoints: `src/Api/Handlers/Issues/IssueEndpoints.cs`
- Handlers: `src/Api/Handlers/Issues/`
- Program: `src/Api/Program.cs`
- Shared: `src/Shared/`
- Validators: `src/Shared/Validators/`

### Search/Filter implementation (2026-02-27)
- Added search and filter capability to Issues list endpoint
- Pattern: MongoDB regex filters with case-insensitive matching using `Builders<T>.Filter.Regex()`
- SearchTerm: filters on Title OR Description (using `Filter.Or()`)
- AuthorName: filters on Author.Name field
- All filters combined with `Filter.And()` along with base Archived filter
- Query string parameters flow: IssueEndpoints → ListIssuesHandler → IssueRepository
- IssueApiClient builds URL with conditional query params using `Uri.EscapeDataString()`
- FluentValidation: optional string parameters with max length 200 chars
- Updated interface signature requires all implementations and test mocks to include new optional params

### Sprint 3 API hardening (2026-02-27)
- **Scalar API Reference UI:** Added `MapScalarApiReference()` after `MapOpenApi()` in Program.cs. Required `global using Scalar.AspNetCore;` in GlobalUsings.cs
- **CORS:** Added CORS policy in Program.cs before `builder.Build()`. Uses `Cors:AllowedOrigins` from config with defaults `["https://localhost:7001", "http://localhost:5001"]`. Call `app.UseCors()` after `UseHttpsRedirection()`
- **Nullable reference warnings:** Fixed 14 CS8603/CS8625 warnings using null-forgiving operator `!` on Result.Value after checking Success/Failure. Pattern: `return result.Value!;` when preceded by Failure check that throws
- **Comment filtering by issueId:** Extended GetAllAsync to accept optional `issueId` parameter. Uses `Builders<Comment>.Filter.Eq(c => c.Issue.Id, objectId)` to filter. Flow: CommentEndpoints → ListCommentsHandler → CommentRepository. CommentApiClient builds URL with query param: `?issueId={Uri.EscapeDataString(issueId)}`
- **MongoDB filter composition:** Use `Builders<T>.Filter.Empty` as base, then add conditional filters with `Eq()`, `Regex()`, etc. Combine with `Filter.And()` or `Filter.Or()` as needed

### Sprint 4 API Versioning (2026-02-28)
- **Asp.Versioning.Http package:** Added v8.1.0 to Directory.Packages.props in API Documentation section. Package version centralized (no version in Api.csproj)
- **ApiVersioningExtensions:** Created `src/Api/Extensions/ApiVersioningExtensions.cs` with `AddApiVersioning()` builder extension
- **Versioning config:** Default v1.0, AssumeDefaultVersionWhenUnspecified=true, ReportApiVersions=true
- **Multiple readers:** URL segment (primary), X-Api-Version header, api-version query string using `ApiVersionReader.Combine()`
- **Integration:** Called `builder.AddApiVersioning()` in Program.cs after `builder.AddAuth0()`
- **Note:** Did not modify endpoint route patterns — existing `/api/v1/` prefixes already handle URL segment versioning. Just wired infrastructure.

### Sprint 4 CurrentUserService (2026-02-27)
- **ICurrentUserService interface:** Created `src/Api/Services/ICurrentUserService.cs` with properties: UserId, Name, Email, IsAuthenticated
- **CurrentUserService implementation:** Reads Auth0 claims from HttpContext.User. Checks both standard claim types (ClaimTypes.NameIdentifier) and Auth0-specific claim names ("sub", "name", "email")
- **Services directory:** Created new `src/Api/Services/` directory for infrastructure services
- **CurrentUserExtensions:** Created `src/Api/Extensions/CurrentUserExtensions.cs` with `AddCurrentUser()` extension method that registers IHttpContextAccessor (scoped) and ICurrentUserService (scoped)
- **DI registration:** Called `builder.Services.AddCurrentUser()` in Program.cs after other service registrations
- **Handler updates:** Injected ICurrentUserService into CreateIssueHandler and CreateCommentHandler. Both now populate Author field from current user when authenticated, falling back to UserDto.Empty when not
- **Author wiring pattern:** Check `_currentUserService.IsAuthenticated`, then construct `new UserDto(UserId ?? "", Name ?? "", Email ?? "")` or use `UserDto.Empty`
- **Build verification:** Api.csproj builds successfully. Test failures expected (Gimli's scope to fix test instantiation)
- **Note:** Did NOT add [Authorize] attributes — that's s4-api-policies task scope

### CancellationToken propagation (2026-03-01)
- **Bug pattern:** Handler accepting `CancellationToken` but not forwarding it to the repository call — silent discard
- **Fix location:** `ListCategoriesHandler.Handle()` line 38 — `GetAllAsync()` → `GetAllAsync(cancellationToken)`
- **Verification step:** Always grep handler `Handle()` calls to confirm `cancellationToken` flows into every async repository call
- **Scope:** `ListStatusesHandler` was already correct; only `ListCategoriesHandler` was affected
- **Interface check:** `ICategoryRepository.GetAllAsync(CancellationToken = default)` already had the right signature — purely a call-site omission

### mongodb-dba-patterns skill created (2026-03-03)
- Created `.squad/skills/mongodb-dba-patterns/SKILL.md` to replace the deleted `.github/instructions/mongo-dba.instructions.md`
- Skill covers: replica set management, database/collection creation, backup/restore (mongodump/mongorestore), performance tuning (indexes, profiling), security (SCRAM-SHA-256, roles, TLS), and MongoDB 7.x+ upgrade guidance
- Confidence set to `low` (first formal establishment of this skill)
- Preferred tooling: VS Code MongoDB extension and MongoDB Compass over raw shell commands

### Sprint 4 API Authorization Policies (2026-02-28)
- **Authorization applied:** Added `.RequireAuthorization()` to all 12 write endpoints (POST, PATCH, DELETE) across IssueEndpoints, CategoryEndpoints, StatusEndpoints, and CommentEndpoints
- **Read-only endpoints:** GET endpoints (list, getById) remain public — no authorization required for read operations
- **Pattern:** Chain `.RequireAuthorization()` after the last `.Produces()` call in the endpoint fluent configuration
- **Scope:** Only modified endpoint mapping files (`*Endpoints.cs`), no changes to handlers or other files
- **Build verification:** Api.csproj builds successfully with no errors or warnings

### Sprint issue #80: ObjectId standardization (2026-03-04)
- **Commands/Queries fixed:** DeleteIssueCommand (string→ObjectId), UpdateIssueCommand (ObjectId?+bug→ObjectId), DeleteCategoryCommand (ObjectId?+bug→ObjectId), UpdateCategoryCommand/DeleteStatusCommand/UpdateStatusCommand/DeleteCommentCommand/UpdateCommentCommand (removed invalid = string.Empty defaults), UpdateIssueStatusCommand.IssueId
- **GetQuery types changed:** GetIssueQuery, GetCategoryQuery, GetStatusQuery, GetCommentQuery — all changed from string to ObjectId parameter; GetCommentQuery had been ObjectId already but handler body was using string operations (broken code)
- **Handler pattern after fix:** Delete/Update handlers receive command.Id as ObjectId directly — no TryParse. Get handlers receive ObjectId in query record directly — no TryParse.
- **Endpoint pattern established:** All GET-by-id, PATCH, DELETE endpoints now have if (!ObjectId.TryParse(id, out var objectId)) return Results.BadRequest("Invalid ID format"); BEFORE creating the command/query
- **Cascading Web fix:** Blazor pages that created commands with Id = routeParamString needed Id = ObjectId.Parse(routeParamString). Added @using MongoDB.Bson to _Imports.razor
- **Edit tool unreliability:** The dit tool reports "File updated with changes" even when old_str doesn't match. Use PowerShell Set-Content with regex (-replace) for reliable bulk text replacement across many files
- **Pre-push gate note:** Test files (Gimli scope) had cascading compilation errors; pushed with --no-verify per issue instructions that test failures are expected and assigned to #82-88
- **PR:** #91 — do NOT merge until Gimli's test issues (#82, #84, #86, #88) are resolved
