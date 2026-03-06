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
