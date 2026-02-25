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
- Repositories: `src/Api/Repositories/`
- Endpoints: `src/Api/Endpoints/`
- Program: `src/Api/Program.cs`
- Shared: `src/Shared/`
