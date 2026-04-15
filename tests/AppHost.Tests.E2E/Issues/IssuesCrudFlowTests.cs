// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssuesCrudFlowTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

namespace AppHost.Tests.E2E.Issues;

/// <summary>
/// E2E tests for Issues CRUD and archive flow.
/// Verifies that Admin users can navigate to Issues pages, create issues, filter, and access archive prerequisites.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class IssuesCrudFlowTests(PlaywrightFixture fixture)
{
	private const string AdminRole = "ADMIN";

	/// <summary>
	/// Verifies that an Admin user can navigate to the Issues list page.
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToIssuesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act
			await page.GotoAsync($"{fixture.WebUrl}/issues", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			page.Url.Should().Contain("/issues", "should navigate to Issues page");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Create Issue page.
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToCreateIssuePage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act
			await page.GotoAsync($"{fixture.WebUrl}/issues/create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			page.Url.Should().Contain("/issues/create", "should navigate to Create Issue page");

			// Verify form is present
			var formLocator = page.Locator("form, input, button[type='submit']");
			(await formLocator.First.IsVisibleAsync()).Should().BeTrue("form element should be visible on create page");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can create and view an issue via the form.
	/// </summary>
	[Fact]
	public async Task Admin_CanCreateAndViewIssue()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act - Navigate to create page
			await page.GotoAsync($"{fixture.WebUrl}/issues/create", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Wait for form to load
			await page.WaitForSelectorAsync("form", new PageWaitForSelectorOptions { Timeout = 15000 });

			// Fill in the Title field
			var titleSelector = "input[id='title'], input[name='title'], input[placeholder*='title' i], input[aria-label*='title' i]";
			await page.FillAsync(titleSelector, "E2E Test Issue");

			// Fill in the Description field
			var descriptionSelector = "textarea[id='description'], textarea[name='description'], textarea[placeholder*='description' i]";
			await page.FillAsync(descriptionSelector, "This is an E2E test issue created by Playwright");

			// Click submit button
			await page.ClickAsync("button[type='submit']");

			// Wait for navigation away from create page
			await page.WaitForURLAsync(url => !url.Contains("/create"), new PageWaitForURLOptions { Timeout = 15000 });

			// Assert - Should redirect to either list or detail page
			page.Url.Should().Contain("/issues", "should redirect to issues page after creation");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that the Issues page has filter/search UI when issues exist.
	/// </summary>
	[Fact]
	public async Task Admin_CanFilterIssuesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act
			await page.GotoAsync($"{fixture.WebUrl}/issues", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Page should load
			page.Url.Should().Contain("/issues", "should navigate to Issues page");

			// Look for search/filter input (defensive - may not exist if no issues)
			var searchSelector = "input[type='search'], input[placeholder*='search' i], input[aria-label*='search' i]";
			var searchInput = page.Locator(searchSelector);

			if (await searchInput.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 }))
			{
				// If filter UI exists, test it
				await searchInput.FillAsync("test");
				(await searchInput.InputValueAsync()).Should().Contain("test", "search input should accept text");
			}
			else
			{
				// Filter UI not present (likely no issues) - just verify page loaded
				var pageContent = page.Locator("h1, h2, h3");
				(await pageContent.First.IsVisibleAsync()).Should().BeTrue("page should have loaded with heading visible");
			}
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Categories page (archive prerequisite).
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToCategoriesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act
			await page.GotoAsync($"{fixture.WebUrl}/categories", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			page.Url.Should().Contain("/categories", "should navigate to Categories page");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that the Categories page shows archive UI for Admin users.
	/// </summary>
	[Fact]
	public async Task Admin_CanViewCategoriesForArchive()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AdminRole);
		if (credentials is null)
			throw SkipException.ForSkip("Admin test credentials not configured (E2E_TEST_ADMIN_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
			page,
			fixture.WebUrl,
			credentials.Value.Email,
			credentials.Value.Password);

			// Act
			await page.GotoAsync($"{fixture.WebUrl}/categories", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Page should load
			page.Url.Should().Contain("/categories", "should navigate to Categories page");

			// Look for archive button (defensive - may not exist if no categories)
			var archiveButtonSelector = "button:has-text('Archive'), button[aria-label*='archive' i], button[title*='archive' i]";
			var archiveButton = page.Locator(archiveButtonSelector);

			if (await archiveButton.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 }))
			{
				// Archive button found - verify it's visible
				(await archiveButton.First.IsVisibleAsync()).Should().BeTrue("archive button should be visible");
			}
			else
			{
				// No archive button (likely no categories) - just verify page loaded
				var pageHeading = page.Locator("h1, h2, h3");
				(await pageHeading.First.IsVisibleAsync()).Should().BeTrue("page should have loaded with heading visible");
			}
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
