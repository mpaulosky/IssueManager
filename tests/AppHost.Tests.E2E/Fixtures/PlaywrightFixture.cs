// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     PlaywrightFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

namespace AppHost.Tests.E2E.Fixtures;

/// <summary>
/// Playwright fixture that hosts the Aspire AppHost for end-to-end tests.
/// Initializes Playwright and provides browser/page instances for E2E tests.
/// Starts the Aspire AppHost and captures the Web app URL for browser navigation.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PlaywrightFixture : IAsyncLifetime
{
	private IPlaywright? _playwright;
	private IBrowser? _browser;
	private IDistributedApplicationTestingBuilder? _builder;
	private DistributedApplication? _app;
	private string? _webUrl;
	private ResourceNotificationService? _notificationService;

	/// <summary>
	/// Gets the Playwright instance.
	/// </summary>
	public IPlaywright Playwright =>
		_playwright ?? throw new InvalidOperationException("Playwright not initialized. Check IsAvailable.");

	/// <summary>
	/// Gets the browser instance.
	/// </summary>
	public IBrowser Browser =>
		_browser ?? throw new InvalidOperationException("Browser not initialized. Check IsAvailable.");

	/// <summary>
	/// Gets the base URL of the web application.
	/// </summary>
	public string WebUrl =>
		_webUrl ?? throw new InvalidOperationException("Web URL not available. Check IsAvailable.");

	/// <summary>
	/// Gets the Aspire app instance.
	/// </summary>
	public DistributedApplication App =>
		_app ?? throw new InvalidOperationException("Aspire app not initialized. Check IsAvailable.");

	/// <summary>
	/// True if the fixture was successfully initialized.
	/// </summary>
	public bool IsAvailable { get; private set; }

	/// <summary>
	/// The reason initialization was skipped or failed, if IsAvailable is false.
	/// </summary>
	public string? UnavailableReason { get; private set; }

	/// <summary>
	/// Creates a new browser context with isolated state for test isolation.
	/// </summary>
	public async Task<IBrowserContext> NewContextAsync(BrowserNewContextOptions? options = null)
	{
		return await Browser.NewContextAsync(options ?? new BrowserNewContextOptions
		{
			IgnoreHTTPSErrors = true
		});
	}

	/// <summary>
	/// Creates a new page in a fresh browser context for test isolation.
	/// </summary>
	public async Task<IPage> NewPageAsync()
	{
		var context = await NewContextAsync();
		return await context.NewPageAsync();
	}

	public async ValueTask InitializeAsync()
	{
		try
		{
			// Step 1: Initialize Aspire AppHost
			_builder = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.AppHost>(CancellationToken.None);

			_builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
			{
				clientBuilder.AddStandardResilienceHandler();
			});

			_app = await _builder.BuildAsync(CancellationToken.None);

			_notificationService = _app.Services.GetRequiredService<ResourceNotificationService>();

			// Start the distributed application
			await _app.StartAsync(CancellationToken.None);

			// Wait for the web app to be running
			await _notificationService.WaitForResourceAsync(
				Website,
				KnownResourceStates.Running,
				CancellationToken.None).WaitAsync(TimeSpan.FromMinutes(3));

			// Get the web app URL
			_webUrl = _app.GetEndpoint(Website, "https")?.ToString()
				?? _app.GetEndpoint(Website, "http")?.ToString();

			if (string.IsNullOrEmpty(_webUrl))
			{
				IsAvailable = false;
				UnavailableReason = "Could not get web app endpoint URL";
				return;
			}

			// Step 2: Initialize Playwright
			_playwright = await Microsoft.Playwright.Playwright.CreateAsync();

			// Launch Chromium in headless mode
			_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
			{
				Headless = true
			});

			IsAvailable = true;
		}
		catch (Exception ex)
		{
			IsAvailable = false;
			UnavailableReason = $"Fixture initialization failed: {ex.Message}";
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_browser is not null)
		{
			await _browser.CloseAsync();
		}

		_playwright?.Dispose();

		if (_app is not null)
		{
			await _app.StopAsync();
			await _app.DisposeAsync();
		}
	}
}
