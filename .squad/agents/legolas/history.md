# Legolas — History

## Core Context
Frontend Developer on IssueManager (.NET 10, Blazor Interactive Server Rendering, Tailwind CSS, bUnit). User: Matthew Paulosky.

## Learnings

### Project setup
- Blazor project: `src/Web/`
- Component naming: `*Component.razor` and `*Page.razor`
- Stream rendering enabled project-wide
- bUnit tests in: `tests/Web.Tests.Bunit/`
- Tailwind CSS configured in the Web project

### Sprint 2 CRUD Pages Implementation (2026-02-27)
- Page structure patterns: PageTitle, container div with max-w-7xl mx-auto px-4 py-8, card-based layouts
- Route conventions: `/issues`, `/issues/create`, `/issues/{Id}`, `/issues/{Id}/edit` (and similar for categories/statuses)
- Component injection via `@inject` directives (IIssueApiClient, ICategoryApiClient, etc.)
- Form handling: Use local form models (mutable classes) instead of init-only command records for `@bind-Value` compatibility
- DataTable and Pagination components: reusable shared components with RenderFragment<TItem> patterns
- StatusBadge: dynamic color styling based on status name
- Theme switcher: Applied FOUC fix by moving theme IIFE to `<head>` in App.razor before other scripts
- NavMenu: Desktop and mobile sections must be updated separately when adding new links
- UserDto property: Uses `Name` not `DisplayName`
- Error handling: `StateHasChanged()` requires `await InvokeAsync(StateHasChanged)` in async contexts

### Sprint 3 Comment Edit/Delete Features (2026-02-27)
- Enhanced IssueDetailPage with inline comment editing: Click "Edit" shows textarea with Save/Cancel buttons
- Delete confirmation: Integrated existing ConfirmDialog component for comment deletion with proper state management
- Comment filtering: Implemented client-side filtering by IssueId using LINQ `.Where()` clause (API doesn't support server-side filtering yet)
- Edit state management: Track `_editingCommentId`, `_editCommentText`, and `_isSavingComment` for inline editing
- Delete state management: Track `_showDeleteDialog` and `_commentToDeleteId` for confirmation flow
- API methods: Used `UpdateAsync(id, UpdateCommentCommand)` and `DeleteAsync(id)` from ICommentApiClient
- UpdateCommentCommand: Requires `Id`, `Title`, and `CommentText` properties
- Lambda expressions in Razor: Use `() => StartEditComment(comment)` for passing parameters in event handlers
- TODO comment: Added reminder to restrict edit/delete buttons to comment owner when auth is implemented

### Sprint 4 Auth Protection (2026-02-27)
- Added login/logout buttons to NavMenu: AuthorizeView with desktop and mobile sections showing user name when authenticated
- Protected CRUD pages with [Authorize]: CreateIssuePage, EditIssuePage, CreateCategoryPage, EditCategoryPage, CreateStatusPage, EditStatusPage
- Created RedirectToLoginPage.razor: Simple redirect to /auth/login for unauthorized access
- Updated Routes.razor: Changed RouteView to AuthorizeRouteView with NotAuthorized handler
- Added global usings: Microsoft.AspNetCore.Authorization and Microsoft.AspNetCore.Components.Authorization in _Imports.razor
- Auth endpoints: /auth/login and /auth/logout already wired by Sam (backend)
- List/view pages intentionally left public: IssuesPage, IssueDetailPage, CategoriesPage, StatusesPage

### Auth UI Visibility Fixes (2026-02-27)
- Added `@rendermode InteractiveServer` to NavMenu.razor: Enables ThemeToggle, ThemeColorSelector, and mobile hamburger to work (previously static SSR made @onclick non-functional)
- NavMenu auth visibility: Wrapped "New Issue" links (desktop + mobile) in `<AuthorizeView><Authorized>` blocks
- NavMenu admin visibility: Wrapped "Categories" and "Statuses" links (desktop + mobile) in `<AuthorizeView Roles="Admin"><Authorized>` blocks
- IssuesPage: Added `@rendermode InteractiveServer`, injected AuthenticationStateProvider, loaded current user name, and restricted Edit links to Admin role OR issue author
- IssueDetailPage: Added `@rendermode InteractiveServer`, injected AuthenticationStateProvider, loaded current user name, and restricted Edit Issue button to Admin role OR issue author
- Auth pattern: Use `<AuthorizeView Roles="Admin"><Authorized>` for admin-only, and `<NotAuthorized>` with manual author name check (`_currentUserName == issue.Author.Name`) for author access
- Interactive components must have `@rendermode InteractiveServer` to support @onclick handlers and JS interop (IJSRuntime)


### Web Project Architecture & Testing (2026-03-06)
- Web project now uses Vertical Slice Architecture — features are self-contained slices with their own routes, pages, components, and services
- Old horizontal-layer structure (Handlers/, Pages/, Services/) replaced with feature-based folder organization
- Test project renamed: Blazor.Tests → Web.Tests.Bunit (path: 	ests/Web.Tests.Bunit/)
- All test references should use the new project name

### Dependabot Lockfile Review — PR #113 (2026-04-12)
- `src/Web/package.json` only declares Tailwind build dependencies; `picomatch` is transitive from `@parcel/watcher` in the frontend toolchain
- Safe lockfile-only security PRs in `src/Web/` can be validated with green CI plus a clean `npm ci --ignore-scripts` run in an isolated worktree
- Dependabot lockfile bumps may add nested wasm helper entries in `package-lock.json` without changing `package.json`; treat that as acceptable when the target package version is correct and install validation stays clean
- AppHost E2E cancellations were present in PR #113, but the required checks for the dependency bump were green and the PR remained safe to merge

### 2026-04-12: Dependabot picomatch Security Bump (PR #113)
- Reviewed and merged PR #113 updating picomatch 4.0.3 -> 4.0.4 in src/Web/package-lock.json
- Security patch for picomatch advisories; CI validation passed
- Tailwind optional wasm helper entries updated as expected lockfile churn
- Merged to main; created reusable `.squad/skills/dependabot-lockfile-review/` skill for future Web dependency reviews
- Decision recorded in `.squad/decisions.md`

### 2026-04-12: E2E Playwright Tests for Issues CRUD (PR #142)
- Implemented 6 E2E test scenarios in `tests/AppHost.Tests.E2E/Issues/IssuesCrudFlowTests.cs`
- Test patterns: Use `[Collection("PlaywrightE2E")]` attribute, `PlaywrightFixture fixture` constructor parameter, and `try/finally` with page context cleanup
- All tests check fixture availability and test credentials before running
- Defensive testing: Assertions check for UI element existence, not specific data (works with empty database)
- Route pattern: `/issues`, `/issues/create`, `/issues/{Id}/edit`, `/categories`
- Auth pattern: Use `Auth0LoginHelper.GetTestCredentials("ADMIN")` and `Auth0LoginHelper.LoginAsync(page, testBaseUrl, username, password, 30000)`
- Timeout strategy: Most WaitFor operations use 15000ms, Auth0LoginHelper uses 30000ms default
- FluentAssertions style: `.Should().BeTrue()`, `.Should().Contain()`, `.Should().NotBeNull()`
- GlobalUsings: E2E project has global usings for Xunit, FluentAssertions, Playwright, fixtures, and helpers — no additional using statements needed
- Build verification: Tests build successfully with warnings consistent with existing E2E tests
