namespace IssueManager.E2E.PageObjects;

/// <summary>
/// Page object model for the issue form (create/edit).
/// </summary>
public class IssueFormPage
{
	private readonly IPage _page;

	public IssueFormPage(IPage page)
	{
		_page = page;
	}

	/// <summary>
	/// Navigates to the issue creation page.
	/// </summary>
	public async Task GotoCreateAsync()
	{
		await _page.GotoAsync("/issues/create");
	}

	/// <summary>
	/// Navigates to the issue edit page.
	/// </summary>
	public async Task GotoEditAsync(string issueId)
	{
		await _page.GotoAsync($"/issues/{issueId}/edit");
	}

	/// <summary>
	/// Fills the issue title field.
	/// </summary>
	public async Task FillTitleAsync(string title)
	{
		await _page.FillAsync("#title", title);
	}

	/// <summary>
	/// Fills the issue description field.
	/// </summary>
	public async Task FillDescriptionAsync(string description)
	{
		await _page.FillAsync("#description", description);
	}

	/// <summary>
	/// Selects the issue status.
	/// </summary>
	public async Task SelectStatusAsync(IssueStatus status)
	{
		await _page.SelectOptionAsync("#status", status.ToString());
	}

	/// <summary>
	/// Clicks the submit button.
	/// </summary>
	public async Task SubmitAsync()
	{
		await _page.ClickAsync("button[type='submit']");
	}

	/// <summary>
	/// Clicks the cancel button.
	/// </summary>
	public async Task CancelAsync()
	{
		await _page.ClickAsync("button:has-text('Cancel')");
	}

	/// <summary>
	/// Checks if a validation error is visible for a specific field.
	/// </summary>
	public async Task<bool> IsValidationErrorVisibleAsync(string errorText)
	{
		return await _page.Locator($"text={errorText}").IsVisibleAsync();
	}

	/// <summary>
	/// Checks if the submit button is disabled.
	/// </summary>
	public async Task<bool> IsSubmitButtonDisabledAsync()
	{
		return await _page.Locator("button[type='submit']").IsDisabledAsync();
	}

	/// <summary>
	/// Checks if the form is in submitting state (spinner visible).
	/// </summary>
	public async Task<bool> IsSubmittingAsync()
	{
		return await _page.Locator(".spinner-border").IsVisibleAsync();
	}

	/// <summary>
	/// Gets the button text (Create Issue or Update Issue).
	/// </summary>
	public async Task<string> GetSubmitButtonTextAsync()
	{
		return await _page.Locator("button[type='submit']").TextContentAsync() ?? string.Empty;
	}
}
