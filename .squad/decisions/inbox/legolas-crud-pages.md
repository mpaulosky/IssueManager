# Sprint 2 CRUD Pages Decision Log

**Date:** 2026-02-27  
**Agent:** Legolas (Frontend Developer)  
**Requested by:** Matthew Paulosky

## Decision: Page Structure and Routing Conventions

**What:** Established consistent structure and routing patterns for all CRUD pages (Issues, Categories, Statuses).

**Why:** Consistency improves maintainability and developer experience. Predictable routes match REST conventions.

**Details:**
- **Routing:** `/{resource}` (list), `/{resource}/create` (create), `/{resource}/{Id}` (detail), `/{resource}/{Id}/edit` (edit)
- **Namespaces:** `Web.Pages.Issues`, `Web.Pages.Categories`, `Web.Pages.Statuses`
- **File naming:** `IssuesPage.razor`, `CreateIssuePage.razor`, `IssueDetailPage.razor`, `EditIssuePage.razor`
- **StreamRendering:** Applied `@attribute [StreamRendering]` to all list pages for progressive rendering
- **Container pattern:** All pages use `max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8` for consistent spacing

---

## Decision: Form Binding with Init-Only Properties

**What:** Created mutable form model classes (e.g., `CategoryFormModel`) instead of directly binding to init-only command records.

**Why:** Blazor's `@bind-Value` requires settable properties. The `CreateCategoryCommand`, `UpdateCategoryCommand`, etc., have `init` setters which are incompatible with two-way binding.

**Solution:**
- Define private nested classes with `{ get; set; }` properties + DataAnnotations
- On submit, map form model properties to command records
- Validation rules duplicated from Shared.Validators but necessary for Blazor EditForm

**Example:**
```csharp
private class CategoryFormModel
{
	[Required]
	[StringLength(100, MinimumLength = 2)]
	public string CategoryName { get; set; } = string.Empty;

	[StringLength(500)]
	public string? CategoryDescription { get; set; }
}

private async Task HandleSubmit()
{
	var command = new CreateCategoryCommand
	{
		CategoryName = _formModel.CategoryName,
		CategoryDescription = _formModel.CategoryDescription
	};
	await CategoryClient.CreateAsync(command);
}
```

---

## Decision: Theme Switcher FOUC Fix

**What:** Moved the theme IIFE (immediately invoked function expression) from the end of `<body>` to the top of `<head>` in `App.razor`.

**Why:** Prevents Flash of Unstyled Content (FOUC) by applying dark mode and color theme BEFORE the page renders, rather than after Blazor hydrates.

**Before:**
```html
<body>
	<script>
		(function() { /* apply theme */ })();
	</script>
</body>
```

**After:**
```html
<head>
	<script>
		(function() { /* apply theme */ })();
	</script>
	<!-- other scripts -->
</head>
```

Additionally, added error suppression in `ThemeToggle.razor` and `ThemeColorSelector.razor` via `try-catch (JSException)` to handle race conditions where JS interop isn't ready on first render.

---

## Decision: NavMenu Updates

**What:** Added Categories and Statuses links to both desktop and mobile navigation sections in `NavMenu.razor`.

**Why:** Users need access to manage categories and statuses, not just issues. Both desktop and mobile menus must be updated separately because they use different markup structures.

**Implementation:**
- Desktop: Added links after "New Issue" in the `hidden md:flex` section
- Mobile: Added links after "New Issue" in the `@if (_mobileMenuOpen)` block

---

## Decision: UserDto Property Reference

**What:** Used `UserDto.Name` instead of `UserDto.DisplayName` in all pages.

**Why:** The actual DTO has a `Name` property, not `DisplayName`. This was discovered during build error resolution.

**Affected pages:**
- `IssuesPage.razor` (table row)
- `IssueDetailPage.razor` (header and comments section)

---

## Future Considerations

1. **Search/Filter:** The `IssuesPage.razor` has filter controls but they're not yet wired up to API params. Sam will need to extend `IssueApiClient.GetAllAsync` to accept `searchTerm`, `statusName`, `categoryName` parameters.
2. **Comments filtering:** `IssueDetailPage.razor` loads ALL comments and notes that filtering by issue ID will be added when the API supports it.
3. **Validation duplication:** Form model validation is duplicated from Shared.Validators. Consider using FluentValidation for Blazor forms in the future.
4. **Delete confirmations:** Categories and Statuses use `ConfirmDialog` component. Issues pages don't have delete functionality yet — to be added in future sprint.

---

**Status:** ✅ Complete  
**Build:** ✅ Passing (0 errors, 59 warnings)  
**Files Created:** 10 page files (IssuesPage, CreateIssuePage, IssueDetailPage, EditIssuePage, CategoriesPage, CreateCategoryPage, EditCategoryPage, StatusesPage, CreateStatusPage, EditStatusPage)  
**Files Modified:** NavMenu.razor, ThemeToggle.razor, ThemeColorSelector.razor, App.razor
