# Gandalf — History

## Project Context
- **Project:** IssueManager
- **Stack:** .NET 10, C# 14, Blazor (Interactive Server Rendering), MongoDB, EF Core, CQRS/MediatR, Vertical Slice Architecture, .NET Aspire
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/IssueManager
- **Auth:** Auth0 for Authentication and Authorization
- **Joined:** 2026-02-27

## Day-1 Context
IssueManager is a .NET 10 Blazor Server application backed by MongoDB. It uses CQRS with MediatR and a Vertical Slice Architecture. The team has been building out repositories, handlers, endpoints, and tests. Auth0 is the designated identity provider for both authentication and authorization.

Key security concerns to address from day one:
- Auth0 integration completeness (SDK, flows, RBAC)
- All API endpoints and Blazor pages must enforce authorization
- MongoDB query safety (NoSQL injection vectors)
- No secrets in source — User Secrets and Key Vault patterns required
- Antiforgery token enforcement confirmed in Program.cs

## Learnings
<!-- Gandalf appends learnings here after each session -->

### 2026-02-27: Auth0 Scaffold Implementation — Passive Configuration Pattern
**Sprint:** Sprint 3 Hardening
**Branch:** feat/sprint-3-hardening
**Status:** Complete
**What:** Scaffolded Auth0 infrastructure for both API (JWT Bearer) and Web (OIDC) projects using passive configuration pattern. Created extension methods that check for config presence before wiring up authentication.
**Key Implementation:**
- Added `Auth0.AspNetCore.Authentication` v1.5.0 and `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 to Directory.Packages.props
- Created `Api/Extensions/AuthExtensions.cs` with JWT Bearer setup (validates Auth0:Domain and Auth0:Audience)
- Created `Web/Extensions/AuthExtensions.cs` with OIDC setup (validates Auth0:Domain and Auth0:ClientId)
- Both extensions return early if config is missing — safe to call without secrets
- Wired `builder.AddAuth0()` into both Program.cs files after `AddServiceDefaults()`
- Created comprehensive setup guide at `docs/auth0-setup.md` with tenant setup and user secrets instructions
**Why:** P0 security gap identified in copilot-instructions audit. Auth0 was designated but completely absent. Passive pattern allows team to work while Matthew obtains credentials.
**Coordination:** Worked in parallel with Sam (API endpoints) and Legolas (UI). Added single line to Program.cs files to minimize merge conflicts.
**Security Note:** Applications run in "open mode" (no auth enforced) until secrets are configured. This is intentional for staged rollout.

### 2026-02-27: Auth0 Middleware Activation (s4-auth0)
**Sprint:** Sprint 4
**Branch:** feat/sprint-4-auth
**Status:** Complete
**What:** Activated Auth0 middleware pipeline in both API and Web projects. Wired up UseAuthentication/UseAuthorization middleware, added login/logout endpoints for Web, and created TokenForwardingHandler to attach Bearer tokens to outgoing API requests.
**Key Implementation:**
- Added `UseAuthentication()` and `UseAuthorization()` to both Api/Program.cs and Web/Program.cs middleware pipelines
- Created `/auth/login` and `/auth/logout` endpoints in Web/Program.cs using Auth0's LoginAuthenticationPropertiesBuilder and LogoutAuthenticationPropertiesBuilder
- Created `Web/Services/TokenForwardingHandler.cs` — a DelegatingHandler that reads the access_token from HttpContext and attaches it as a Bearer token to outgoing API requests
- Registered TokenForwardingHandler as transient service and added it to all four HttpClient registrations (Issue, Category, Status, Comment)
- Added necessary using statements: Auth0.AspNetCore.Authentication, Microsoft.AspNetCore.Authentication, Microsoft.AspNetCore.Authentication.Cookies
**Pipeline Order:**
- Api: UseHttpsRedirection → UseCors → UseAuthentication → UseAuthorization → endpoints
- Web: UseHttpsRedirection → UseStaticFiles → UseAuthentication → UseAuthorization → UseAntiforgery → endpoints
**Why:** The passive Auth0 extensions were in place but the middleware pipeline was not wired up. Without UseAuthentication/UseAuthorization, auth checks don't run. The TokenForwardingHandler ensures that when a user is logged into the Web app, their access token is automatically attached to backend API calls.
**Build Status:** Build succeeded (45 warnings, 0 errors). Fixed unrelated ApiVersioningExtensions.cs compilation error by removing incompatible AddApiExplorer call.
**Security Note:** Endpoints still don't enforce authorization (no [Authorize] attributes yet) — that's the next todo (s4-api-policies). This change activates the middleware pipeline so that when policies are added, the infrastructure is ready.


