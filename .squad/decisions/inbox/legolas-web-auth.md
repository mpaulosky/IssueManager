# Decision: Sprint 4 Web Auth Protection

**Date:** 2026-02-27
**Author:** Legolas (Frontend Developer)
**Status:** Implemented

## Context
Auth0 OIDC integration was wired by Sam in the Web project (AddAuth0, UseAuthentication, UseAuthorization). Login/logout endpoints exist at `/auth/login` and `/auth/logout`. The UI needed to expose these endpoints and protect CRUD pages from anonymous users.

## Decision
1. **NavMenu login/logout UI**: Added `<AuthorizeView>` blocks in both desktop and mobile sections showing:
   - When authenticated: User name + "Log out" link
   - When not authenticated: "Log in" link
2. **CRUD page protection**: Added `@attribute [Authorize]` to all create/edit pages (6 pages total):
   - CreateIssuePage, EditIssuePage
   - CreateCategoryPage, EditCategoryPage
   - CreateStatusPage, EditStatusPage
3. **Redirect for unauthorized**: Created `RedirectToLoginPage.razor` and updated `Routes.razor` to use `AuthorizeRouteView` with `<NotAuthorized>` handler
4. **Global usings**: Added `Microsoft.AspNetCore.Authorization` and `Microsoft.AspNetCore.Components.Authorization` to `_Imports.razor`

## Rationale
- List/view pages (IssuesPage, IssueDetailPage, etc.) intentionally left public for anonymous browsing
- Only mutating operations (create/edit) require authentication
- AuthorizeView provides cascading auth state without additional injections
- RedirectToLoginPage with `forceLoad: true` ensures full page reload to trigger OIDC flow

## Impact
- Build: Clean (0 errors, 0 warnings after adding missing using directives)
- User experience: Unauthenticated users can browse issues but must log in to create/edit
- Future: Comment edit/delete buttons should check if comment author matches logged-in user (deferred to future sprint)

## Files Changed
- `src/Web/Layout/NavMenu.razor` (added AuthorizeView blocks)
- `src/Web/Pages/Issues/CreateIssuePage.razor` (added [Authorize])
- `src/Web/Pages/Issues/EditIssuePage.razor` (added [Authorize])
- `src/Web/Pages/Categories/CreateCategoryPage.razor` (added [Authorize])
- `src/Web/Pages/Categories/EditCategoryPage.razor` (added [Authorize])
- `src/Web/Pages/Statuses/CreateStatusPage.razor` (added [Authorize])
- `src/Web/Pages/Statuses/EditStatusPage.razor` (added [Authorize])
- `src/Web/Pages/RedirectToLoginPage.razor` (created)
- `src/Web/Routes.razor` (changed RouteView to AuthorizeRouteView)
- `src/Web/_Imports.razor` (added Authorization namespaces)
