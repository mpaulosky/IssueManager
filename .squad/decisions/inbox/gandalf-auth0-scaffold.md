# Decision: Auth0 Scaffold — Passive Configuration Pattern

**Date:** 2026-02-27
**By:** Gandalf (Security Officer)
**Branch:** feat/sprint-3-hardening
**Status:** Implemented

## Context
Auth0 was designated as the identity provider in `.github/copilot-instructions.md` but was completely absent from the codebase (P0 security gap from copilot-instructions audit). Configuration secrets were not yet available from Matthew, but infrastructure could be scaffolded in preparation.

## Decision
Implement Auth0 authentication extensions using a **passive configuration pattern**:
- Extensions check for required config values (Auth0:Domain, Auth0:ClientId/Audience) before setup
- If config is missing, extensions return early without throwing
- Applications run in "open mode" (no authentication enforced) until secrets are configured
- Both API (JWT Bearer) and Web (OIDC) authentication scaffolded simultaneously

## Implementation
1. **Package Versions** (Directory.Packages.props):
   - `Auth0.AspNetCore.Authentication` v1.5.0 (Web OIDC)
   - `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 (API JWT)

2. **API Extension** (`Api/Extensions/AuthExtensions.cs`):
   - Validates `Auth0:Domain` and `Auth0:Audience` config presence
   - Configures JWT Bearer authentication with RS256 signature validation
   - Returns early if config missing (graceful degradation)

3. **Web Extension** (`Web/Extensions/AuthExtensions.cs`):
   - Validates `Auth0:Domain` and `Auth0:ClientId` config presence
   - Configures Auth0 OIDC authentication for Blazor
   - Adds `CascadingAuthenticationState` for Blazor components
   - Returns early if config missing (graceful degradation)

4. **Wiring**: Single `builder.AddAuth0()` call in both `Program.cs` files after `AddServiceDefaults()`

5. **Documentation**: Created `docs/auth0-setup.md` with:
   - Auth0 tenant setup instructions (application + API registration)
   - User secrets configuration for local dev
   - GitHub Actions secrets checklist
   - Verification steps

## Rationale
- **Passive pattern** allows parallel development by other agents while secrets are being obtained
- **Graceful degradation** prevents build breaks or startup failures
- **Single line integration** minimizes merge conflicts with Sam's API work and Legolas's UI work
- **Documentation-first** ensures Matthew has clear setup path when credentials arrive

## Security Notes
- Applications currently run in **open mode** (no auth enforcement)
- This is intentional for staged rollout — NOT a security vulnerability at this stage
- Once secrets are configured:
  - API will validate JWT signatures against Auth0 public keys
  - Web will redirect unauthenticated users to Auth0 Universal Login
- Future work required:
  - Sam must add `.RequireAuthorization()` to API endpoints
  - Legolas must wire Login/Logout buttons in navigation
  - Role-based authorization (RBAC) enforcement

## Dependencies
- Blocked by: Matthew obtaining Auth0 tenant credentials
- Blocks: Authorization enforcement (Sam), Login UI (Legolas)

## Testing
- Build validated: No Auth0-related compilation errors
- Pre-existing errors from other team members' work remain (IssueDetailPage, MapScalarApiReference)
- Passive pattern tested: Extensions return early without exception when config absent

## Alternatives Considered
1. **Throw exception if config missing** — Rejected: Breaks build for team members without secrets
2. **Environment variable gating** — Rejected: More complex than config presence check
3. **Separate PR for each project** — Rejected: Creates coordination overhead; Auth0 setup is atomic
