namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for issue creation workflow.
/// </summary>
public class IssueCreationTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public IssueCreationTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_CanNavigateToCreateIssuePage()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act
		await formPage.GotoCreateAsync();

		// Assert
		var title = await page.TitleAsync();
		title.Should().Contain("Create");
		var buttonText = await formPage.GetSubmitButtonTextAsync();
		buttonText.Should().Contain("Create Issue");
	}

	[Fact]
	public async Task User_CanCreateIssueWithValidData()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Test Issue {timestamp}";
		var issueDescription = "This is a test issue created by E2E tests.";

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.FillDescriptionAsync(issueDescription);
		await formPage.SelectStatusAsync(IssueStatus.Open);
		await formPage.SubmitAsync();

		// Assert - should redirect to issue list or detail page
		await page.WaitForURLAsync(url => url.Contains("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().NotContain("/create");
	}

	[Fact]
	public async Task User_SeesValidationErrorForEmptyTitle()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(""); // Empty title
		await formPage.FillDescriptionAsync("Some description");
		await formPage.SubmitAsync();

		// Assert
		var validationErrorVisible = await formPage.IsValidationErrorVisibleAsync("Title is required");
		validationErrorVisible.Should().BeTrue();
	}

	[Fact]
	public async Task User_SeesValidationErrorForTitleTooShort()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync("AB"); // Too short (min 3 chars)
		await formPage.FillDescriptionAsync("Some description");
		await formPage.SubmitAsync();

		// Assert
		var validationErrorVisible = await formPage.IsValidationErrorVisibleAsync("must be between 3 and 200");
		validationErrorVisible.Should().BeTrue();
	}

	[Fact]
	public async Task User_CanCreateIssueWithMinimalData()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Minimal Issue {timestamp}";

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		// No description - should be optional
		await formPage.SubmitAsync();

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().NotContain("/create");
	}

	[Fact]
	public async Task User_CanCancelIssueCreation()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync("Test Issue");
		await formPage.CancelAsync();

		// Assert - should navigate away from create page
		await page.WaitForURLAsync(url => !url.Contains("/create"));
		var currentUrl = page.Url;
		currentUrl.Should().NotContain("/create");
	}

	[Theory]
	[InlineData(IssueStatus.Open)]
	[InlineData(IssueStatus.InProgress)]
	[InlineData(IssueStatus.Closed)]
	public async Task User_CanCreateIssueWithDifferentStatuses(IssueStatus status)
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Issue {status} {timestamp}";

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);
		await formPage.SelectStatusAsync(status);
		await formPage.SubmitAsync();

		// Assert
		await page.WaitForURLAsync(url => url.Contains("/issues"));
		var currentUrl = page.Url;
		currentUrl.Should().NotContain("/create");
	}
}
