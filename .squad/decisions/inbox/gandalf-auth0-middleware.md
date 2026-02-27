# Auth0 Middleware Pipeline Activation

**Date:** 2026-02-27  
**By:** Gandalf (Security Officer)  
**Sprint:** Sprint 4  
**Branch:** feat/sprint-4-auth  
**Status:** Complete  

## Decision

Activated the Auth0 authentication and authorization middleware pipeline in both the API and Web projects, completing the Auth0 integration started in Sprint 3.

## Context

The passive Auth0 extensions (`AddAuth0()`) were already in place from Sprint 3, which registered the authentication services but did not activate the middleware pipeline. Without `UseAuthentication()` and `UseAuthorization()` in the middleware chain, authentication checks cannot run even if credentials are configured.

Additionally, the Web project (Blazor Server with OIDC) needed login/logout endpoints to initiate and terminate user sessions, and a mechanism to forward the user's access token to backend API calls.

## Changes Made

### 1. API Middleware Pipeline (src/Api/Program.cs)
Added authentication and authorization middleware after CORS:
```csharp
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
```

### 2. Web Middleware Pipeline (src/Web/Program.cs)
Added authentication and authorization middleware between static files and antiforgery:
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
```

### 3. Login/Logout Endpoints (src/Web/Program.cs)
Added Auth0-based login and logout endpoints:
```csharp
app.MapGet("/auth/login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();
    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authProperties);
});

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    var authProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri("/")
        .Build();
    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});
```

Required using statements:
- `Auth0.AspNetCore.Authentication`
- `Microsoft.AspNetCore.Authentication`
- `Microsoft.AspNetCore.Authentication.Cookies`

### 4. Token Forwarding Handler (src/Web/Services/TokenForwardingHandler.cs)
Created a new `DelegatingHandler` that automatically attaches the user's access token to outgoing API requests:
```csharp
public sealed class TokenForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 5. HttpClient Registration (src/Web/Program.cs)
Registered the handler and attached it to all HttpClient instances:
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwardingHandler>();

builder.Services.AddHttpClient<IIssueApiClient, IssueApiClient>(client =>
    client.BaseAddress = new Uri("https+http://api"))
    .AddServiceDiscovery()
    .AddHttpMessageHandler<TokenForwardingHandler>();

// Same pattern applied to Category, Status, and Comment clients
```

## Rationale

### Middleware Order
The order of middleware matters in ASP.NET Core:
- **UseAuthentication** must come before **UseAuthorization** — it populates HttpContext.User
- **UseStaticFiles** should come before authentication — static files don't need auth checks
- **UseAntiforgery** comes after authorization — it needs the user identity for token validation

### Token Forwarding Pattern
The `TokenForwardingHandler` follows the standard ASP.NET Core delegating handler pattern for propagating tokens:
- Uses `IHttpContextAccessor` to access the current user's HttpContext
- Retrieves the `access_token` saved by Auth0 during OIDC login
- Attaches it as a Bearer token to every outgoing API request
- Fails gracefully if no token is present (e.g., user not logged in)

This ensures that when a logged-in user interacts with the Blazor UI, their identity is automatically propagated to backend API calls.

### Login/Logout Endpoints
- `/auth/login?returnUrl=/` initiates the OIDC Authorization Code flow with Auth0
- `/auth/logout` signs the user out of both Auth0 and the local cookie session
- These endpoints will be linked from Blazor components (e.g., NavMenu) in a future task

## Impact

- **Authentication pipeline is now active** — when Auth0 secrets are configured, the middleware will validate tokens and populate HttpContext.User
- **Authorization is ready** — when `[Authorize]` attributes or policies are added to endpoints, they will be enforced
- **Token propagation works** — logged-in users' API calls will carry their Bearer token automatically
- **No breaking changes** — applications still run in "open mode" until policies are added (s4-api-policies task)

## Next Steps

1. Add `[Authorize]` attributes or `.RequireAuthorization()` to API endpoints (s4-api-policies)
2. Add `<AuthorizeView>` components to Blazor pages
3. Configure Auth0 secrets in User Secrets and test end-to-end authentication flow
4. Add UI elements (Login/Logout buttons) to NavMenu

## Build Status

✅ Build succeeded: 45 warnings, 0 errors

**Note:** Fixed an unrelated compilation error in `ApiVersioningExtensions.cs` (removed incompatible `AddApiExplorer` call) to ensure the build passed.

## References

- Auth0 ASP.NET Core SDK: https://github.com/auth0/auth0-aspnetcore-authentication
- Token Forwarding Pattern: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#outgoing-request-middleware
- ASP.NET Core Middleware Order: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
