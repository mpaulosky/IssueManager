# Decision: ProfilePage bUnit Test Patterns

**By:** Gimli (Tester)
**Date:** 2026-07-14
**Status:** Informational

## Context

Wrote 8 bUnit tests for `ProfilePage.razor.cs` in `tests/Web.Tests.Bunit/Components/Features/Profile/ProfilePageTests.cs`.

## Decisions Made

### Standalone class (not ComponentTestBase)

ProfilePage reads the authenticated username from `AuthenticationStateProvider`. The base `ComponentTestBase` calls `AddAuthorization()` without `SetAuthorized`, giving an anonymous user. ProfilePage needs a named user. Use standalone `BunitContext` with `AddAuthorization().SetAuthorized("testuser")`, matching the AdminPageTests pattern.

### Username fallback coverage

To test the `?? "User"` fallback, call `ctx.AddAuthorization()` without `SetAuthorized`. bUnit's TestAuthorizationContext returns an anonymous `ClaimsPrincipal` with `Identity.Name = null`, triggering the fallback — without needing to mock `AuthenticationStateProvider` directly.

### Heading assertions use TextContent not Markup.Contain

The heading `@_userName's Profile` contains an apostrophe that may be HTML-encoded differently across bUnit versions. Use `cut.Find("h1").TextContent.Should().Contain("testuser")` instead of `cut.Markup.Should().Contain("testuser's Profile")` to be encoding-agnostic.

### No service registrations beyond IIssueApiClient

`IssueCard` (rendered per issue) injects `NavigationManager` only (auto-provided by bUnit). `StatusBadge` is a pure component with no DI. Neither `ICategoryApiClient` nor `IStatusApiClient` are needed for ProfilePage tests.
