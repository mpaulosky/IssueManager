// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UnauthenticatedNavigationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using AppHost.Tests.E2E.Helpers;

namespace AppHost.Tests.E2E.Navigation;

/// <summary>
/// E2E tests for unauthenticated user navigation behavior.
/// Verifies that anonymous users see login button and protected routes redirect to login.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class UnauthenticatedNavigationTests(PlaywrightFixture fixture)
{
	/// <summary>
	/// Verifies that an unauthenticated user sees the login button.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_SeesLoginButton()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			(await Auth0LoginHelper.IsLoginButtonVisibleAsync(page)).Should().BeTrue("Unauthenticated user should see login button");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an unauthenticated user does NOT see the logout button.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_DoesNotSeeLogoutButton()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeFalse("Unauthenticated user should NOT see logout button");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an unauthenticated user does NOT see the "New Issue" link.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_DoesNotSeeNewIssueLink()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert
			var newIssueLink = page.Locator("a[href='/issues/create']");
			(await newIssueLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Unauthenticated user should NOT see New Issue link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an unauthenticated user does NOT see admin-only menu items.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_DoesNotSeeAdminOnlyMenuItems()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Verify we are in unauthenticated state before checking menu visibility
			var loginLink = page.Locator("a[href='/auth/login']");
			await loginLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

			// Assert
			var categoriesLink = page.Locator("a[href='/categories']");
			var statusesLink = page.Locator("a[href='/statuses']");
			var adminLink = page.Locator("a[href='/admin']");
			var sampleDataLink = page.Locator("a[href='/sample-data']");

			(await categoriesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Unauthenticated user should NOT see Categories link");
			(await statusesLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Unauthenticated user should NOT see Statuses link");
			(await adminLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Unauthenticated user should NOT see Admin link");
			(await sampleDataLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
				.Should().BeFalse("Unauthenticated user should NOT see Sample Data link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that navigating to a protected route redirects to login.
	/// The /issues/create route requires authorization.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_ProtectedRouteRedirectsToLogin()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act - Try to navigate directly to a protected page
			await page.GotoAsync($"{fixture.WebUrl.TrimEnd('/')}/issues/create",
				new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Should redirect to login or show redirect-to-login component
			// The app uses a RedirectToLoginPage component that navigates to /auth/login
			var currentUrl = page.Url;

			// Either redirected to Auth0 login or to our /auth/login endpoint
			var isRedirectedToLogin = currentUrl.Contains("auth0.com") ||
				currentUrl.Contains("/auth/login") ||
				currentUrl.Contains("/u/login");

			isRedirectedToLogin.Should().BeTrue(
				$"Protected route should redirect to login, but URL is: {currentUrl}");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that the Issues page is accessible without authentication.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_CanAccessIssuesPage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync($"{fixture.WebUrl.TrimEnd('/')}/issues",
				new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Should stay on issues page (not redirect)
			page.Url.Should().Contain("/issues");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that the home page is accessible without authentication.
	/// </summary>
	[Fact]
	public async Task Unauthenticated_CanAccessHomePage()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Playwright fixture unavailable");

		var page = await fixture.NewPageAsync();

		try
		{
			// Act
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Should not redirect to login
			page.Url.Should().NotContain("auth0.com");
			page.Url.Should().NotContain("/auth/login");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
