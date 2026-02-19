namespace IssueManager.E2E.Fixtures;

/// <summary>
/// Provides Playwright browser and page lifecycle management for E2E tests.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
	private IPlaywright? _playwright;
	private IBrowser? _browser;

	/// <summary>
	/// Gets the base URL for the application under test.
	/// </summary>
	public string BaseUrl { get; } = Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5000";

	/// <summary>
	/// Gets the browser instance.
	/// </summary>
	public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");

	/// <summary>
	/// Gets the Playwright instance.
	/// </summary>
	public IPlaywright Playwright => _playwright ?? throw new InvalidOperationException("Playwright not initialized");

	/// <summary>
	/// Initializes Playwright and launches the browser.
	/// </summary>
	public async Task InitializeAsync()
	{
		_playwright = await Microsoft.Playwright.Playwright.CreateAsync();
		_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = true,
			Args = new[] { "--disable-dev-shm-usage" }
		});
	}

	/// <summary>
	/// Creates a new browser page for testing.
	/// </summary>
	public async Task<IPage> CreatePageAsync()
	{
		var context = await Browser.NewContextAsync(new BrowserNewContextOptions
		{
			BaseURL = BaseUrl,
			ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
		});
		return await context.NewPageAsync();
	}

	/// <summary>
	/// Disposes the browser and Playwright instance.
	/// </summary>
	public async Task DisposeAsync()
	{
		if (_browser is not null)
		{
			await _browser.CloseAsync();
			await _browser.DisposeAsync();
		}

		_playwright?.Dispose();
	}
}
