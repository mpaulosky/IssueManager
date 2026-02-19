namespace IssueManager.E2E.PageObjects;

/// <summary>
/// Page object model for the issue detail page.
/// </summary>
public class IssueDetailPage
{
	private readonly IPage _page;

	public IssueDetailPage(IPage page)
	{
		_page = page;
	}

	/// <summary>
	/// Navigates to the issue detail page.
	/// </summary>
	public async Task GotoAsync(string issueId)
	{
		await _page.GotoAsync($"/issues/{issueId}");
	}

	/// <summary>
	/// Gets the issue title.
	/// </summary>
	public async Task<string> GetIssueTitleAsync()
	{
		return await _page.Locator("h1.issue-title").TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Gets the issue description.
	/// </summary>
	public async Task<string> GetIssueDescriptionAsync()
	{
		return await _page.Locator(".issue-description").TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Gets the current issue status.
	/// </summary>
	public async Task<string> GetIssueStatusAsync()
	{
		return await _page.Locator(".status-badge").TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Clicks the edit button.
	/// </summary>
	public async Task ClickEditButtonAsync()
	{
		await _page.ClickAsync("text=Edit");
	}

	/// <summary>
	/// Clicks the delete button.
	/// </summary>
	public async Task ClickDeleteButtonAsync()
	{
		await _page.ClickAsync("text=Delete");
	}

	/// <summary>
	/// Clicks the back to list button.
	/// </summary>
	public async Task ClickBackToListAsync()
	{
		await _page.ClickAsync("text=Back to List");
	}

	/// <summary>
	/// Updates the issue status using the status dropdown.
	/// </summary>
	public async Task UpdateStatusAsync(IssueStatus newStatus)
	{
		await _page.SelectOptionAsync("#status-selector", newStatus.ToString());
		await _page.ClickAsync("button:has-text('Update Status')");
	}

	/// <summary>
	/// Checks if a success message is visible.
	/// </summary>
	public async Task<bool> IsSuccessMessageVisibleAsync()
	{
		return await _page.Locator(".alert-success").IsVisibleAsync();
	}

	/// <summary>
	/// Checks if the created/updated timestamp is visible.
	/// </summary>
	public async Task<bool> IsTimestampVisibleAsync()
	{
		return await _page.Locator(".issue-metadata").IsVisibleAsync();
	}
}
