// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthorNavigationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using AppHost.Tests.E2E.Helpers;

namespace AppHost.Tests.E2E.Navigation;

/// <summary>
/// E2E tests for Author role navigation behavior.
/// Verifies that Author users see appropriate menu items and do NOT see admin-only items.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class AuthorNavigationTests(PlaywrightFixture fixture)
{
	private const string AuthorRole = "AUTHOR";

	/// <summary>
	/// Verifies that an Author user can successfully log in via Auth0.
	/// </summary>
	[Fact]
	public async Task Author_CanLoginSuccessfully()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AuthorRole);
		if (credentials is null)
			throw SkipException.ForSkip("Author test credentials not configured (E2E_TEST_AUTHOR_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			var loginSuccess = await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Assert
			loginSuccess.Should().BeTrue("Author should be able to log in successfully");
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Page should show logged-in state");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Author user sees appropriate menu items (Home, Issues, New Issue).
	/// </summary>
	[Fact]
	public async Task Author_SeesAppropriateMenuItems()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AuthorRole);
		if (credentials is null)
			throw SkipException.ForSkip("Author test credentials not configured (E2E_TEST_AUTHOR_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Act - Navigate to home page to see the nav menu
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Author should see basic menu items
			var homeLink = page.Locator("a[href='/']").First;
			var issuesLink = page.Locator("a[href='/issues']");
			var newIssueLink = page.Locator("a[href='/issues/create']");

			(await homeLink.IsVisibleAsync()).Should().BeTrue("Author should see Home link");
			(await issuesLink.IsVisibleAsync()).Should().BeTrue("Author should see Issues link");
			(await newIssueLink.IsVisibleAsync()).Should().BeTrue("Author should see New Issue link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Author user does NOT see admin-only menu items.
	/// </summary>
	[Fact]
	public async Task Author_DoesNotSeeAdminOnlyMenuItems()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AuthorRole);
		if (credentials is null)
			throw SkipException.ForSkip("Author test credentials not configured (E2E_TEST_AUTHOR_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Act - Navigate to home page to see the nav menu
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Author should NOT see admin-only menu items
			var categoriesLink = page.Locator("a[href='/categories']");
			var statusesLink = page.Locator("a[href='/statuses']");
			var adminLink = page.Locator("a[href='/admin']");
			var sampleDataLink = page.Locator("a[href='/sample-data']");

			(await categoriesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Author should NOT see Categories link");
			(await statusesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Author should NOT see Statuses link");
			(await adminLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Author should NOT see Admin link");
			(await sampleDataLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Author should NOT see Sample Data link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Author user can navigate to the Issues page.
	/// </summary>
	[Fact]
	public async Task Author_CanNavigateToIssuesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AuthorRole);
		if (credentials is null)
			throw SkipException.ForSkip("Author test credentials not configured (E2E_TEST_AUTHOR_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Act
			await page.ClickAsync("a[href='/issues']");
			await page.WaitForURLAsync("**/issues**");

			// Assert
			page.Url.Should().Contain("/issues");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Author user can navigate to the New Issue page.
	/// </summary>
	[Fact]
	public async Task Author_CanNavigateToNewIssuePage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(AuthorRole);
		if (credentials is null)
			throw SkipException.ForSkip("Author test credentials not configured (E2E_TEST_AUTHOR_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Act
			await page.ClickAsync("a[href='/issues/create']");
			await page.WaitForURLAsync("**/issues/create**");

			// Assert
			page.Url.Should().Contain("/issues/create");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
