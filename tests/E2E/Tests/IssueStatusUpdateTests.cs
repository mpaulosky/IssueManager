namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for issue status update workflow.
/// </summary>
public class IssueStatusUpdateTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public IssueStatusUpdateTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_CanUpdateIssueStatusFromDetailPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Status Update Test {timestamp}";

		// Create an issue with Open status
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SelectStatusAsync(IssueStatus.Open);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);
		await detailPage.UpdateStatusAsync(IssueStatus.InProgress);

		// Assert
		var updatedStatus = await detailPage.GetIssueStatusAsync();
		updatedStatus.Should().Contain("InProgress");
	}

	[Fact]
	public async Task User_CanUpdateIssueStatusToClosedFromDetailPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Close Issue Test {timestamp}";

		// Create an issue
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SelectStatusAsync(IssueStatus.InProgress);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);
		await detailPage.UpdateStatusAsync(IssueStatus.Closed);

		// Assert
		var updatedStatus = await detailPage.GetIssueStatusAsync();
		updatedStatus.Should().Contain("Closed");
	}

	[Fact]
	public async Task User_CanEditIssueFromDetailPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var detailPage = new IssueDetailPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Edit Test {timestamp}";
		var updatedTitle = $"Updated {issueTitle}";

		// Create an issue
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		// Get issue ID
		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var issueId = page.Url.Split("/issues/").Last().Split("/").First();

		// Act
		await detailPage.GotoAsync(issueId);
		await detailPage.ClickEditButtonAsync();

		// Update the issue
		await formPage.FillTitleAsync(updatedTitle);
		await formPage.SubmitAsync();

		// Assert
		await page.WaitForURLAsync(url => url.Contains($"/issues/{issueId}"));
		await detailPage.GotoAsync(issueId);
		var displayedTitle = await detailPage.GetIssueTitleAsync();
		displayedTitle.Should().Contain(updatedTitle);
	}
}
