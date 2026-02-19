namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for issue detail view workflow.
/// </summary>
public class IssueDetailTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public IssueDetailTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_CanViewIssueDetails()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Detail Test Issue {timestamp}";
		var issueDescription = "This issue is for detail view testing.";

		// Create an issue first
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.FillDescriptionAsync(issueDescription);
		await formPage.SubmitAsync();

		// Extract issue ID from URL after creation
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);

		// Assert
		var displayedTitle = await detailPage.GetIssueTitleAsync();
		displayedTitle.Should().Contain(issueTitle);

		var displayedDescription = await detailPage.GetIssueDescriptionAsync();
		displayedDescription.Should().Contain(issueDescription);
	}

	[Fact]
	public async Task User_CanNavigateToEditFromDetailPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Edit Navigation Test {timestamp}";

		// Create an issue first
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);
		await detailPage.ClickEditButtonAsync();

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/edit"));
		var currentUrl = page.Url;
		currentUrl.Should().Contain("/edit");
	}

	[Fact]
	public async Task User_CanNavigateBackToListFromDetailPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Back Navigation Test {timestamp}";

		// Create an issue first
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);
		await detailPage.ClickBackToListAsync();

		// Assert
		await page.WaitForURLAsync(url => url == "/issues" || url.EndsWith("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().Match(url => url.EndsWith("/issues"));
	}

	[Fact]
	public async Task User_CanSeeIssueMetadata()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Metadata Test {timestamp}";

		// Create an issue first
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);

		// Assert
		var timestampVisible = await detailPage.IsTimestampVisibleAsync();
		timestampVisible.Should().BeTrue();

		var statusVisible = await detailPage.GetIssueStatusAsync();
		statusVisible.Should().NotBeNullOrEmpty();
	}
}
