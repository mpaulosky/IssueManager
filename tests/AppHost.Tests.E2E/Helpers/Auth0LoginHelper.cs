// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Auth0LoginHelper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

namespace AppHost.Tests.E2E.Helpers;

/// <summary>
/// Helper for authenticating with Auth0 via browser-based OIDC flow in Playwright tests.
/// Test credentials are read from environment variables for security.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Auth0LoginHelper
{
	/// <summary>
	/// Gets test credentials from environment variables.
	/// </summary>
	public static (string Email, string Password)? GetTestCredentials(string role)
	{
		var suffix = role.ToUpperInvariant();
		var email = Environment.GetEnvironmentVariable($"E2E_TEST_{suffix}_EMAIL");
		var password = Environment.GetEnvironmentVariable($"E2E_TEST_{suffix}_PASSWORD");

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			return null;
		}

		return (email, password);
	}

	/// <summary>
	/// Performs Auth0 login via the browser-based OIDC flow.
	/// Navigates to /auth/login, fills Auth0 form, and waits for redirect back.
	/// </summary>
	/// <param name="page">The Playwright page instance.</param>
	/// <param name="baseUrl">The application base URL.</param>
	/// <param name="email">The test user email.</param>
	/// <param name="password">The test user password.</param>
	/// <param name="timeout">Timeout for waiting operations (default 30 seconds).</param>
	/// <returns>True if login succeeded, false otherwise.</returns>
	public static async Task<bool> LoginAsync(
		IPage page,
		string baseUrl,
		string email,
		string password,
		int timeout = 30000)
	{
		try
		{
			// Navigate to the login endpoint which triggers Auth0 redirect
			await page.GotoAsync($"{baseUrl.TrimEnd('/')}/auth/login", new PageGotoOptions
			{
				WaitUntil = WaitUntilState.NetworkIdle,
				Timeout = timeout
			});

			// Wait for Auth0 login page
			await page.WaitForURLAsync("**/u/login**", new PageWaitForURLOptions { Timeout = timeout });

			// Fill in the Auth0 Universal Login form
			await page.FillAsync("input[name='username'], input[name='email'], input#username, input[type='email']", email);
			await page.FillAsync("input[name='password'], input#password, input[type='password']", password);

			// Click the submit button
			await page.ClickAsync("button[type='submit'], button[name='action'], button[data-action-button-primary='true']");

			// Wait for redirect back to our app (should redirect to home or return URL)
			await page.WaitForURLAsync(url =>
				url.Contains(new Uri(baseUrl).Host) && !url.Contains("auth0.com"),
				new PageWaitForURLOptions { Timeout = timeout });

			// Wait for the page to stabilize
			await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			return true;
		}
		catch (TimeoutException)
		{
			return false;
		}
		catch (PlaywrightException)
		{
			return false;
		}
	}

	/// <summary>
	/// Performs logout by navigating to /auth/logout.
	/// </summary>
	/// <param name="page">The Playwright page instance.</param>
	/// <param name="baseUrl">The application base URL.</param>
	/// <param name="timeout">Timeout for waiting operations (default 30 seconds).</param>
	public static async Task LogoutAsync(IPage page, string baseUrl, int timeout = 30000)
	{
		await page.GotoAsync($"{baseUrl.TrimEnd('/')}/auth/logout", new PageGotoOptions
		{
			WaitUntil = WaitUntilState.NetworkIdle,
			Timeout = timeout
		});

		// Wait for redirect back to home
		await page.WaitForURLAsync(url =>
			url.Contains(new Uri(baseUrl).Host) && !url.Contains("auth0.com"),
			new PageWaitForURLOptions { Timeout = timeout });
	}

	/// <summary>
	/// Checks if the current page shows the user as logged in.
	/// </summary>
	/// <param name="page">The Playwright page instance.</param>
	/// <returns>True if logged in (greeting visible), false otherwise.</returns>
	public static async Task<bool> IsLoggedInAsync(IPage page)
	{
		// Look for the "Hello, username!" text or "Log out" link
		var logoutLink = page.Locator("a[href='/auth/logout']");
		return await logoutLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
	}

	/// <summary>
	/// Checks if the login button is visible (indicates logged out state).
	/// </summary>
	/// <param name="page">The Playwright page instance.</param>
	/// <returns>True if login button is visible, false otherwise.</returns>
	public static async Task<bool> IsLoginButtonVisibleAsync(IPage page)
	{
		var loginLink = page.Locator("a[href='/auth/login']");
		return await loginLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
	}
}
