namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for issue list and filtering workflow.
/// </summary>
public class IssueListTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public IssueListTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_CanNavigateToIssueListPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Act
		await listPage.GotoAsync();

		// Assert
		var title = await page.TitleAsync();
		title.Should().Contain("Issues");
		var currentUrl = page.Url;
		currentUrl.Should().Contain("/issues");
	}

	[Fact]
	public async Task User_CanSeeIssueList()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Act
		await listPage.GotoAsync();

		// Assert - list should be visible (even if empty)
		var pageContent = await page.ContentAsync();
		pageContent.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task User_CanClickCreateIssueButton()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Act
		await listPage.GotoAsync();
		await listPage.ClickCreateIssueButtonAsync();

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/create"));
		var currentUrl = page.Url;
		currentUrl.Should().Contain("/issues/create");
	}

	[Fact]
	public async Task User_CanFilterIssuesByStatus()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Act
		await listPage.GotoAsync();
		await listPage.FilterByStatusAsync(IssueStatus.Open);

		// Assert - URL should update with filter parameter
		var currentUrl = page.Url;
		currentUrl.Should().Contain("status=Open");
	}

	[Fact]
	public async Task User_CanSearchForIssues()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);
		var searchTerm = "bug";

		// Act
		await listPage.GotoAsync();
		await listPage.SearchAsync(searchTerm);

		// Assert - URL should update with search parameter
		var currentUrl = page.Url;
		currentUrl.Should().Contain($"search={searchTerm}");
	}

	[Fact]
	public async Task User_CanClickOnIssueToViewDetails()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Create a test issue first
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Clickable Issue {timestamp}";

		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		// Act
		await listPage.GotoAsync();
		await listPage.ClickIssueByTitleAsync(issueTitle);

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/issues/") && !url.Contains("/create"));
		var currentUrl = page.Url;
		currentUrl.Should().Match(url => url.Contains("/issues/") && !url.Contains("/issues/create"));
	}
}
