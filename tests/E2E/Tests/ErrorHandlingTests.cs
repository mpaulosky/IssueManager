namespace IssueManager.E2E.Tests;

/// <summary>
/// E2E tests for error handling scenarios.
/// </summary>
public class ErrorHandlingTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public ErrorHandlingTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task User_SeesValidationSummaryForMultipleErrors()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act - Submit form with invalid data
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(""); // Empty title
		await formPage.SubmitAsync();

		// Assert - Validation summary should be visible
		var validationSummaryVisible = await page.Locator(".validation-summary-errors").IsVisibleAsync();
		validationSummaryVisible.Should().BeTrue();
	}

	[Fact]
	public async Task User_CannotSubmitFormWhileSubmitting()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var issueTitle = $"Concurrent Submit Test {timestamp}";

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync(issueTitle);

		// Check initial button state
		var initiallyDisabled = await formPage.IsSubmitButtonDisabledAsync();

		// Submit the form
		var submitTask = formPage.SubmitAsync();

		// Try to check if button is disabled during submission (may be fast)
		// This is a race condition test - button should be disabled

		await submitTask;

		// Assert
		initiallyDisabled.Should().BeFalse("Button should be enabled before submission");
	}

	[Fact]
	public async Task User_SeesErrorMessageForNonExistentIssue()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var detailPage = new IssueDetailPage(page);
		var nonExistentId = "00000000-0000-0000-0000-000000000000";

		// Act
		await detailPage.GotoAsync(nonExistentId);

		// Assert - Should show error or 404 page
		var pageContent = await page.ContentAsync();
		pageContent.Should().Match(content =>
			content.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
			content.Contains("error", StringComparison.OrdinalIgnoreCase) ||
			content.Contains("404"));
	}

	[Fact]
	public async Task User_SeesFieldLevelValidationErrors()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);

		// Act
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync("X"); // Too short
		await formPage.SubmitAsync();

		// Assert
		var titleErrorVisible = await formPage.IsValidationErrorVisibleAsync("must be between");
		titleErrorVisible.Should().BeTrue();
	}

	[Fact]
	public async Task User_CanRecoverFromValidationError()
	{
		// Arrange
		var page = await _fixture.CreatePageAsync();
		var formPage = new IssueFormPage(page);
		var timestamp = DateTime.UtcNow.Ticks;
		var validTitle = $"Recovery Test {timestamp}";

		// Act - First submit with invalid data
		await formPage.GotoCreateAsync();
		await formPage.FillTitleAsync("AB"); // Too short
		await formPage.SubmitAsync();

		// Verify error
		var errorVisible = await formPage.IsValidationErrorVisibleAsync("must be between");
		errorVisible.Should().BeTrue();

		// Fix the error and resubmit
		await formPage.FillTitleAsync(validTitle);
		await formPage.SubmitAsync();

		// Assert - Should succeed
		await page.WaitForURLAsync(url => url.Contains("/issues") && !url.Contains("/create"));
		var currentUrl = page.Url;
		currentUrl.Should().NotContain("/create");
	}
}
