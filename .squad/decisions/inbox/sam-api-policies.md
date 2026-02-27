# API Authorization Policies Applied

**Date:** 2026-02-28  
**Agent:** Sam (Backend Developer)  
**Task:** s4-api-policies  
**Branch:** feat/sprint-4-auth  

## Decision

Added `.RequireAuthorization()` to all write endpoints (POST, PATCH, DELETE) across the API. Read-only GET endpoints remain public.

## Implementation

### Endpoints Modified

**IssueEndpoints.cs:**
- CreateIssue (POST) - now requires authorization
- UpdateIssue (PATCH) - now requires authorization
- DeleteIssue (DELETE) - now requires authorization
- ListIssues (GET) - remains public
- GetIssue (GET) - remains public

**CategoryEndpoints.cs:**
- CreateCategory (POST) - now requires authorization
- UpdateCategory (PATCH) - now requires authorization
- DeleteCategory (DELETE) - now requires authorization
- ListCategories (GET) - remains public
- GetCategory (GET) - remains public

**StatusEndpoints.cs:**
- CreateStatus (POST) - now requires authorization
- UpdateStatus (PATCH) - now requires authorization
- DeleteStatus (DELETE) - now requires authorization
- ListStatuses (GET) - remains public
- GetStatus (GET) - remains public

**CommentEndpoints.cs:**
- CreateComment (POST) - now requires authorization
- UpdateComment (PATCH) - now requires authorization
- DeleteComment (DELETE) - now requires authorization
- ListComments (GET) - remains public
- GetComment (GET) - remains public

### Pattern Used

```csharp
group.MapPost("", async (CreateXCommand command, CreateXHandler handler) =>
{
    // handler logic
})
.WithName("CreateX")
.WithSummary("Create a new X")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.RequireAuthorization();  // ← Added this line
```

## Rationale

- **Write operations security:** All create, update, and delete operations now require authenticated users
- **Public read access:** GET endpoints remain accessible without authentication to support public browsing and integration scenarios
- **Consistent policy:** All four resource types (Issues, Categories, Statuses, Comments) follow the same authorization pattern
- **Minimal API pattern:** Uses built-in `.RequireAuthorization()` method, no need for [Authorize] attributes

## Dependencies

- Auth0 JWT Bearer authentication already configured in Program.cs
- `UseAuthentication()` and `UseAuthorization()` middleware in pipeline
- ICurrentUserService available for handlers to access authenticated user context

## Testing Impact

- Integration tests that POST/PATCH/DELETE will now require valid JWT tokens (Gimli's scope to update tests)
- GET operations continue to work without authentication in tests

## Build Status

✅ Api.csproj builds successfully with no errors or warnings

## Next Steps

- Gimli to update integration tests with Auth0 test tokens
- Consider adding role-based policies (e.g., "Admin" role for delete operations) in future sprint
