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
