// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AdminNavigationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using AppHost.Tests.E2E.Helpers;

namespace AppHost.Tests.E2E.Navigation;

/// <summary>
/// E2E tests for Admin role navigation behavior.
/// Verifies that Admin users can see and access all admin-only menu items.
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("PlaywrightE2E")]
public class AdminNavigationTests(PlaywrightFixture fixture)
{
	private const string AdminRole = "ADMIN";

	/// <summary>
	/// Verifies that an Admin user can successfully log in via Auth0.
	/// </summary>
	[Fact]
	public async Task Admin_CanLoginSuccessfully()
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
			// Act
			var loginSuccess = await Auth0LoginHelper.LoginAsync(
				page,
				fixture.WebUrl,
				credentials.Value.Email,
				credentials.Value.Password);

			// Assert
			loginSuccess.Should().BeTrue("Admin should be able to log in successfully");
			(await Auth0LoginHelper.IsLoggedInAsync(page)).Should().BeTrue("Page should show logged-in state");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user sees all admin menu items (Categories, Statuses, Admin, Sample Data).
	/// </summary>
	[Fact]
	public async Task Admin_SeesAllAdminMenuItems()
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

			// Act - Navigate to home page to see the nav menu
			await page.GotoAsync(fixture.WebUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

			// Assert - Admin should see all admin menu items
			var categoriesLink = page.Locator("a[href='/categories']");
			var statusesLink = page.Locator("a[href='/statuses']");
			var adminLink = page.Locator("a[href='/admin']");
			var sampleDataLink = page.Locator("a[href='/sample-data']");

			(await categoriesLink.IsVisibleAsync()).Should().BeTrue("Admin should see Categories link");
			(await statusesLink.IsVisibleAsync()).Should().BeTrue("Admin should see Statuses link");
			(await adminLink.IsVisibleAsync()).Should().BeTrue("Admin should see Admin link");
			(await sampleDataLink.IsVisibleAsync()).Should().BeTrue("Admin should see Sample Data link");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Categories page.
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
			await page.ClickAsync("a[href='/categories']");
			await page.WaitForURLAsync("**/categories**");

			// Assert
			page.Url.Should().Contain("/categories");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Statuses page.
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToStatusesPage()
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
			await page.ClickAsync("a[href='/statuses']");
			await page.WaitForURLAsync("**/statuses**");

			// Assert
			page.Url.Should().Contain("/statuses");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Admin page.
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToAdminPage()
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
			await page.ClickAsync("a[href='/admin']");
			await page.WaitForURLAsync("**/admin**");

			// Assert
			page.Url.Should().Contain("/admin");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}

	/// <summary>
	/// Verifies that an Admin user can navigate to the Sample Data page.
	/// </summary>
	[Fact]
	public async Task Admin_CanNavigateToSampleDataPage()
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
			await page.ClickAsync("a[href='/sample-data']");
			await page.WaitForURLAsync("**/sample-data**");

			// Assert
			page.Url.Should().Contain("/sample-data");
		}
		finally
		{
			await page.Context.CloseAsync();
		}
	}
}
