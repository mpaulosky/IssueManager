// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UserNavigationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using AppHost.Tests.E2E.Helpers;

namespace AppHost.Tests.E2E.Navigation;

/// <summary>
/// E2E tests for User role navigation behavior.
/// Verifies that basic User role sees appropriate menu items and does NOT see admin-only items.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class UserNavigationTests(PlaywrightFixture fixture)
{
	private const string UserRole = "USER";

	/// <summary>
	/// Verifies that a User can successfully log in via Auth0.
	/// </summary>
	[Fact]
	public async Task User_CanLoginSuccessfully()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(UserRole);
		if (credentials is null)
			throw SkipException.ForSkip("User test credentials not configured (E2E_TEST_USER_EMAIL/PASSWORD)");

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
			loginSuccess.Should().BeTrue("User should be able to log in successfully");
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Page should show logged-in state");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that a User sees appropriate menu items (Home, Issues).
	/// </summary>
	[Fact]
	public async Task User_SeesAppropriateMenuItems()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(UserRole);
		if (credentials is null)
			throw SkipException.ForSkip("User test credentials not configured (E2E_TEST_USER_EMAIL/PASSWORD)");

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

			// Assert - User should see basic menu items
			var homeLink = page.Locator("a[href='/']").First;
			var issuesLink = page.Locator("a[href='/issues']");

			(await homeLink.IsVisibleAsync()).Should().BeTrue("User should see Home link");
			(await issuesLink.IsVisibleAsync()).Should().BeTrue("User should see Issues link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that a User does NOT see admin-only menu items.
	/// </summary>
	[Fact]
	public async Task User_DoesNotSeeAdminOnlyMenuItems()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(UserRole);
		if (credentials is null)
			throw SkipException.ForSkip("User test credentials not configured (E2E_TEST_USER_EMAIL/PASSWORD)");

		var page = await fixture.NewPageAsync();

		try
		{
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify login succeeded before checking menu visibility
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("User should be logged in before checking navigation");

			// Act - Navigate to home page to see the nav menu
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - User should NOT see admin-only menu items
			var categoriesLink = page.Locator("a[href='/categories']");
			var statusesLink = page.Locator("a[href='/statuses']");
			var adminLink = page.Locator("a[href='/admin']");
			var sampleDataLink = page.Locator("a[href='/sample-data']");

			(await categoriesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("User should NOT see Categories link");
			(await statusesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("User should NOT see Statuses link");
			(await adminLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("User should NOT see Admin link");
			(await sampleDataLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("User should NOT see Sample Data link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that a User can navigate to the Issues page.
	/// </summary>
	[Fact]
	public async Task User_CanNavigateToIssuesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(UserRole);
		if (credentials is null)
			throw SkipException.ForSkip("User test credentials not configured (E2E_TEST_USER_EMAIL/PASSWORD)");

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
	/// Verifies that a User sees the "New Issue" link after login (Authorized users can create issues).
	/// </summary>
	[Fact]
	public async Task User_SeesNewIssueLinkAfterLogin()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var credentials = Auth0LoginHelper.GetTestCredentials(UserRole);
		if (credentials is null)
			throw SkipException.ForSkip("User test credentials not configured (E2E_TEST_USER_EMAIL/PASSWORD)");

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

			// Assert - All authenticated users can see "New Issue" link
			var newIssueLink = page.Locator("a[href='/issues/create']");
			(await newIssueLink.IsVisibleAsync()).Should().BeTrue("Authenticated User should see New Issue link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
