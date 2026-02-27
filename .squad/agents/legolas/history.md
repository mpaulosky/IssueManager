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
