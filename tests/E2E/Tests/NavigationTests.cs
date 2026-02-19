namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for navigation and user flows across the application.
/// </summary>
public class NavigationTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public NavigationTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_CanNavigateFromHomeToIssueList()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var homePage = new HomePage(page);

		// Act
		await homePage.GotoAsync();
		await page.ClickAsync("text=Issues");

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().Contain("/issues");
	}

	[Fact]
	public async Task User_CanNavigateCompleteCreateFlow()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Navigation Flow Test {timestamp}";

		// Act - Complete flow: List → Create → Submit → Detail → List
		await listPage.GotoAsync();
		await listPage.ClickCreateIssueButtonAsync();

		await page.WaitForURLAsync(url => url.Contains("/create"));
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SubmitAsync();

		await page.WaitForURLAsync(url => url.Contains("/issues/") && !url.Contains("/create"));

		var detailPage = new IssueDetailPage(page);
		await detailPage.ClickBackToListAsync();

		// Assert
		await page.WaitForURLAsync(url => url.EndsWith("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().Match(url => url.EndsWith("/issues"));
	}

	[Fact]
	public async Task User_CanNavigateToHomeFromAnyPage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var listPage = new IssueListPage(page);

		// Act
		await listPage.GotoAsync();
		await page.ClickAsync("text=Home");

		// Assert
		await page.WaitForURLAsync(url => url == "/" || url.EndsWith("/"));
		var welcomeVisible = await page.Locator("h1:has-text('Welcome to IssueManager')").IsVisibleAsync();
		welcomeVisible.Should().BeTrue();
	}

	[Fact]
	public async Task User_CanNavigateBetweenIssueDetails()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var listPage = new IssueListPage(page);
		var timestamp = DateTime.UtcNow.Ticks;

		// Create two issues
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync($"First Issue {timestamp}");
		await formPage.SubmitAsync();

		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync($"Second Issue {timestamp}");
		await formPage.SubmitAsync();

		// Act - Navigate between issue details via list
		await listPage.GotoAsync();
		await listPage.ClickIssueByTitleAsync($"First Issue {timestamp}");

		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var firstIssueUrl = page.Url;

		var detailPage = new IssueDetailPage(page);
		await detailPage.ClickBackToListAsync();

		await listPage.ClickIssueByTitleAsync($"Second Issue {timestamp}");

		await page.WaitForURLAsync(url => url.Contains("/issues/"));
		var secondIssueUrl = page.Url;

		// Assert
		firstIssueUrl.Should().NotBe(secondIssueUrl);
		secondIssueUrl.Should().Contain("/issues/");
	}
}
