# Decision: CurrentUserService Implementation

**Date:** 2026-02-27  
**By:** Sam (Backend Developer)  
**Task:** s4-current-user (Sprint 4 authentication wiring)

## What

Implemented `ICurrentUserService` to provide access to the currently authenticated user's identity from Auth0 JWT claims.

### Components Created

1. **`src/Api/Services/ICurrentUserService.cs`**
   - Interface with properties: `UserId`, `Name`, `Email`, `IsAuthenticated`
   - UserId returns the Auth0 subject identifier (the unique user ID)

2. **`src/Api/Services/CurrentUserService.cs`**
   - Reads claims from `HttpContext.User` via `IHttpContextAccessor`
   - Falls back to Auth0-specific claim names: "sub", "name", "email"
   - Also checks standard claim types: `ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, `ClaimTypes.Email`

3. **`src/Api/Extensions/CurrentUserExtensions.cs`**
   - Extension method `AddCurrentUser()` registers both `IHttpContextAccessor` and `ICurrentUserService` as scoped services

4. **Handler Integration**
   - `CreateIssueHandler`: Injects `ICurrentUserService`, populates `Author` field from current user
   - `CreateCommentHandler`: Injects `ICurrentUserService`, populates `Author` field from current user

## Why

Auth0 authentication is fully wired in both Api and Web projects. When a user is authenticated, `HttpContext.User` contains Auth0 claims. We need a service abstraction to:
- Provide a clean, testable interface for accessing current user identity
- Avoid direct `HttpContext` access in business logic (handlers)
- Support both standard claim types and Auth0-specific claim names
- Handle unauthenticated requests gracefully (return null/empty values)

## Implementation Pattern

### Claim Reading Strategy
```csharp
// Try standard claim type first, fall back to Auth0-specific name
string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
```

### Author Wiring in Handlers
```csharp
var author = _currentUserService.IsAuthenticated
    ? new UserDto(_currentUserService.UserId ?? string.Empty, 
                  _currentUserService.Name ?? string.Empty, 
                  _currentUserService.Email ?? string.Empty)
    : UserDto.Empty;
```

## Build Status

- **Api.csproj:** ✅ Builds successfully (0 errors, 0 warnings)
- **Test projects:** ❌ 3 compilation errors (expected - test files are Gimli's scope)
  - `CreateIssueHandlerTests.cs` (Unit + Integration)
  - `CreateCommentHandlerTests.cs` (Unit)
  - Tests need to mock/provide `ICurrentUserService` parameter in handler constructors

## Scope Boundaries

**Did NOT implement (out of scope for this task):**
- `[Authorize]` attributes on endpoints — that's s4-api-policies task
- Test file modifications — that's Gimli's responsibility
- UserDto schema changes — Auth0 sub is string, UserDto.Id is string (no changes needed)

## Next Steps

1. **Gimli** needs to update test files to provide mock `ICurrentUserService` in handler constructors
2. **s4-api-policies task** will add `[Authorize]` attributes to endpoints requiring authentication
3. Consider extracting user creation logic into a helper method if pattern repeats across more handlers
