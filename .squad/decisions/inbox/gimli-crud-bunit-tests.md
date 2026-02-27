# Decision: bUnit CRUD Page Tests — Sprint 2

**By:** Gimli (Tester)
**Date:** 2026-02-27
**Status:** FYI (no team decision needed — these are implementation learnings)

## What

Wrote 11 bUnit test files for all 10 Sprint 2 CRUD pages + FooterComponent.

## Key Implementation Decisions

### 1. `BuildInfo` is `internal` — cannot access from test assembly

`BuildInfo` is an `internal static class` in the `Web` namespace, auto-generated at build time.
Test project cannot reference it. Tests for `FooterComponent` use markup assertions (e.g.
`Contain("github.com")`, `Contain("IssueManager")`) instead of `BuildInfo.Version` / `BuildInfo.Commit`.

**Recommendation to Aragorn:** If FooterComponent version/commit text needs to be tested precisely,
consider adding `[assembly: InternalsVisibleTo("BlazorTests")]` to Web project, or exposing
BuildInfo through a service/interface.

### 2. Service registration strategy for page tests

- **Pages using `IssueForm`** (CreateIssuePage, EditIssuePage): inherit `ComponentTestBase`
  because `IssueForm` injects `ICategoryApiClient` + `IStatusApiClient` internally.
- **IssuesPage**: also inherits `ComponentTestBase` (uses category/status for filter dropdowns).
- **IssueDetailPage**: inherits `ComponentTestBase`, adds `IIssueApiClient` + `ICommentApiClient`.
- **Category/Status pages**: fresh `new TestContext()` per class — no `IssueForm`, clean isolation.

### 3. Double-registration pattern works correctly

When `ComponentTestBase` pre-registers `ICategoryApiClient` and a subclass constructor adds it
again, the **last registration wins** in Microsoft DI. This is reliable and the intended approach
when a test class needs its own mock reference.

## Test Coverage Summary

| File | Page | Tests |
|------|------|-------|
| IssuesPageTests | IssuesPage | 6 |
| CreateIssuePageTests | CreateIssuePage | 5 |
| IssueDetailPageTests | IssueDetailPage | 7 |
| EditIssuePageTests | EditIssuePage | 7 |
| CategoriesPageTests | CategoriesPage | 6 |
| CreateCategoryPageTests | CreateCategoryPage | 6 |
| EditCategoryPageTests | EditCategoryPage | 7 |
| StatusesPageTests | StatusesPage | 6 |
| CreateStatusPageTests | CreateStatusPage | 6 |
| EditStatusPageTests | EditStatusPage | 7 |
| FooterComponentTests | FooterComponent | 8 |

**Total: 71 bUnit tests, build: 0 errors, 0 warnings.**
