// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     LogoutTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using AppHost.Tests.E2E.Helpers;

namespace AppHost.Tests.E2E.Navigation;

/// <summary>
/// E2E tests for logout functionality.
/// Verifies that logout works correctly and protected content is hidden after logout.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class LogoutTests(PlaywrightFixture fixture)
{
	private const string AdminRole = "ADMIN";
	private const string AuthorRole = "AUTHOR";
	private const string UserRole = "USER";

	/// <summary>
	/// Verifies that an Admin user can log out successfully.
	/// </summary>
	[Fact]
	public async Task Admin_CanLogoutSuccessfully()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify logged in
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Should be logged in before logout test");

			// Act - Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);

			// Assert
			(await Auth0LoginHelper.IsLoginButtonVisibleAsync(page)).Should().BeTrue("Login button should be visible after logout");
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeFalse("Should not show logged-in state after logout");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that admin menu items are hidden after logout.
	/// </summary>
	[Fact]
	public async Task Admin_MenuItemsHiddenAfterLogout()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify admin menu items are visible
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
			var adminLinkBefore = page.Locator("a[href='/admin']");
			(await adminLinkBefore.IsVisibleAsync()).Should().BeTrue("Admin link should be visible before logout");

			// Act - Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Verify we are in unauthenticated state before checking menu visibility
			var loginLink = page.Locator("a[href='/auth/login']");
			await loginLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

			// Assert - Admin menu items should be hidden
			var categoriesLink = page.Locator("a[href='/categories']");
			var statusesLink = page.Locator("a[href='/statuses']");
			var adminLink = page.Locator("a[href='/admin']");
			var sampleDataLink = page.Locator("a[href='/sample-data']");

			(await categoriesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Categories link should be hidden after logout");
			(await statusesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Statuses link should be hidden after logout");
			(await adminLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Admin link should be hidden after logout");
			(await sampleDataLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Sample Data link should be hidden after logout");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Author user can log out successfully.
	/// </summary>
	[Fact]
	public async Task Author_CanLogoutSuccessfully()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify logged in
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Should be logged in before logout test");

			// Act - Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);

			// Assert
			(await Auth0LoginHelper.IsLoginButtonVisibleAsync(page)).Should().BeTrue("Login button should be visible after logout");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that the "New Issue" link is hidden after logout.
	/// </summary>
	[Fact]
	public async Task Author_NewIssueLinkHiddenAfterLogout()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify "New Issue" link is visible while logged in
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
			var newIssueLinkBefore = page.Locator("a[href='/issues/create']");
			(await newIssueLinkBefore.IsVisibleAsync()).Should().BeTrue("New Issue link should be visible before logout");

			// Act - Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Verify we are in unauthenticated state before checking menu visibility
			var loginLink = page.Locator("a[href='/auth/login']");
			await loginLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

			// Assert
			var newIssueLinkAfter = page.Locator("a[href='/issues/create']");
			(await newIssueLinkAfter.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("New Issue link should be hidden after logout");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that a User can log out successfully.
	/// </summary>
	[Fact]
	public async Task User_CanLogoutSuccessfully()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Verify logged in
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Should be logged in before logout test");

			// Act - Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);

			// Assert
			(await Auth0LoginHelper.IsLoginButtonVisibleAsync(page)).Should().BeTrue("Login button should be visible after logout");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that protected routes redirect to login after logout.
	/// </summary>
	[Fact]
	public async Task User_ProtectedRouteRedirectsAfterLogout()
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
			// First login
			await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Log out
			await Auth0LoginHelper.LogoutAsync(page, fixture.WebUrl);

			// Act - Try to access protected page after logout
			await page.GotoAsync($"{fixture.WebUrl.TrimEnd('/')}/issues/create",
				new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Should redirect to login
			var currentUrl = page.Url;
			var isRedirectedToLogin = currentUrl.Contains("auth0.com") ||
				currentUrl.Contains("/auth/login") ||
				currentUrl.Contains("/u/login");

			isRedirectedToLogin.Should().BeTrue(
				$"Protected route should redirect to login after logout, but URL is: {currentUrl}");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
