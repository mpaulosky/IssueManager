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

