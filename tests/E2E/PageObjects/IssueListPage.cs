namespace IssueManager.E2E.PageObjects;

/// <summary>
/// Page object model for the issue list page.
/// </summary>
public class IssueListPage
{
	private readonly IPage _page;

	public IssueListPage(IPage page)
	{
		_page = page;
	}

	/// <summary>
	/// Navigates to the issue list page.
	/// </summary>
	public async Task GotoAsync()
	{
		await _page.GotoAsync("/issues");
	}

	/// <summary>
	/// Gets the number of visible issues in the list.
	/// </summary>
	public async Task<int> GetIssueCountAsync()
	{
		return await _page.Locator(".issue-item").CountAsync();
	}

	/// <summary>
	/// Clicks on an issue by title.
	/// </summary>
	public async Task ClickIssueByTitleAsync(string title)
	{
		await _page.ClickAsync($"text={title}");
	}

	/// <summary>
	/// Filters issues by status.
	/// </summary>
	public async Task FilterByStatusAsync(IssueStatus status)
	{
		await _page.SelectOptionAsync("#status-filter", status.ToString());
	}

	/// <summary>
	/// Searches for issues by text.
	/// </summary>
	public async Task SearchAsync(string searchText)
	{
		await _page.FillAsync("#search", searchText);
		await _page.PressAsync("#search", "Enter");
	}

	/// <summary>
	/// Checks if an issue with the given title is visible.
	/// </summary>
	public async Task<bool> IsIssueTitleVisibleAsync(string title)
	{
		return await _page.Locator($"text={title}").IsVisibleAsync();
	}

	/// <summary>
	/// Clicks the "Create Issue" button.
	/// </summary>
	public async Task ClickCreateIssueButtonAsync()
	{
		await _page.ClickAsync("text=Create Issue");
	}

	/// <summary>
	/// Gets the status badge text for a specific issue.
	/// </summary>
	public async Task<string> GetIssueStatusAsync(string issueTitle)
	{
		var issueRow = _page.Locator($"text={issueTitle}").Locator("..");
		return await issueRow.Locator(".status-badge").TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Navigates to the next page of results.
	/// </summary>
	public async Task GoToNextPageAsync()
	{
		await _page.ClickAsync("text=Next");
	}

	/// <summary>
	/// Checks if pagination is visible.
	/// </summary>
	public async Task<bool> IsPaginationVisibleAsync()
	{
		return await _page.Locator(".pagination").IsVisibleAsync();
	}
}
