// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     PlaywrightFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

using Testcontainers.MongoDb;

namespace AppHost.Tests.E2E.Fixtures;

/// <summary>
/// Playwright fixture that hosts the Aspire AppHost for end-to-end tests.
/// Initializes Playwright and provides browser/page instances for E2E tests.
/// Starts the Aspire AppHost against a MongoDB test container and captures the Web app URL for browser navigation.
/// If the host can't start, initialization throws and every test in the collection fails.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PlaywrightFixture : IAsyncLifetime
{
	private const string MongoDbImage = "mongo:latest";

	private MongoDbContainer? _mongoContainer;
	private IPlaywright? _playwright;
	private IBrowser? _browser;
	private IDistributedApplicationTestingBuilder? _builder;
	private DistributedApplication? _app;
	private string? _webUrl;

	/// <summary>
	/// Gets the Playwright instance.
	/// </summary>
	public IPlaywright Playwright =>
		_playwright ?? throw new InvalidOperationException("Playwright not initialized.");

	/// <summary>
	/// Gets the browser instance.
	/// </summary>
	public IBrowser Browser =>
		_browser ?? throw new InvalidOperationException("Browser not initialized.");

	/// <summary>
	/// Gets the base URL of the web application.
	/// </summary>
	public string WebUrl =>
		_webUrl ?? throw new InvalidOperationException("Web URL not available.");

	/// <summary>
	/// Gets the Aspire app instance.
	/// </summary>
	public DistributedApplication App =>
		_app ?? throw new InvalidOperationException("Aspire app not initialized.");

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
		// No try/catch: a fixture that can't start must fail every test in the
		// collection, not skip them, so a broken host can't pass CI unnoticed.

		// Step 1: Start a throwaway MongoDB for the API. The AppHost expects the
		// Atlas URI as the issuemanagerdb connection string; without it the API
		// fails to start and the web app never comes up.
		_mongoContainer = new MongoDbBuilder(MongoDbImage).Build();
		await _mongoContainer.StartAsync();

		// Step 2: Initialize Aspire AppHost. Keep the launchSettings ports rather
		// than the testing builder's random ones: Auth0 only accepts callback URLs
		// on its allowed list, so the web app must run on a known port to log in.
		_builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(
			[
				$"--ConnectionStrings:{DatabaseName}={_mongoContainer.GetConnectionString()}",
				"--DcpPublisher:RandomizePorts=false"
			],
			CancellationToken.None);

		_builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
		{
			clientBuilder.AddStandardResilienceHandler();
		});

		_app = await _builder.BuildAsync(CancellationToken.None);

		// Start the app and wait for the web app's health check, with one timeout
		// so unhealthy containers don't hang CI. The wait throws as soon as the web
		// app or a dependency fails to start, rather than running out the clock.
		using var startCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
		await _app.StartAsync(startCts.Token);
		await _app.ResourceNotifications.WaitForResourceHealthyAsync(Website, startCts.Token);

		// Get the web app URL
		_webUrl = _app.GetEndpoint(Website, "https")?.ToString()
			?? _app.GetEndpoint(Website, "http")?.ToString()
			?? throw new InvalidOperationException("Could not get web app endpoint URL");

		// Step 3: Initialize Playwright
		_playwright = await Microsoft.Playwright.Playwright.CreateAsync();

		// Launch Chromium in headless mode
		_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = true
		});
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

		if (_mongoContainer is not null)
		{
			await _mongoContainer.DisposeAsync();
		}
	}
}
