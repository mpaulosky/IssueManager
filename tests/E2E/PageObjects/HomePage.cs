namespace IssueManager.E2E.PageObjects;

/// <summary>
/// Page object model for the home page.
/// </summary>
public class HomePage
{
	private readonly IPage _page;

	public HomePage(IPage page)
	{
		_page = page;
	}

	/// <summary>
	/// Navigates to the home page.
	/// </summary>
	public async Task GotoAsync()
	{
		await _page.GotoAsync("/");
	}

	/// <summary>
	/// Checks if the welcome message is visible.
	/// </summary>
	public async Task<bool> IsWelcomeMessageVisibleAsync()
	{
		return await _page.Locator("h1:has-text('Welcome to IssueManager')").IsVisibleAsync();
	}

	/// <summary>
	/// Gets the page title.
	/// </summary>
	public async Task<string> GetPageTitleAsync()
	{
		return await _page.TitleAsync();
	}
}
