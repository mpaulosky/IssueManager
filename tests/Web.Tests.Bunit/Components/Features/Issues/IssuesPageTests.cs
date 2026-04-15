// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssuesPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Issues;

/// <summary>
/// bUnit tests for the <see cref="IssuesPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class IssuesPageTests : ComponentTestBase
{
	private readonly IIssueApiClient _mockIssueClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="IssuesPageTests"/> class,
	/// registering a mock <see cref="IIssueApiClient"/> in the DI container.
	/// </summary>
	public IssuesPageTests()
	{
		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(PaginatedResponse<IssueDto>.Empty));
		TestContext.Services.AddSingleton(_mockIssueClient);
	}

	private static IssueDto MakeIssue(string title = "Test Issue") => new(
		ObjectId.GenerateNewId(),
		title,
		"Test Description",
		DateTime.UtcNow,
		null,
		new UserDto("user1", "Author Name", "author@test.com"),
		new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug category", DateTime.UtcNow, null, false, UserDto.Empty),
		new StatusDto(ObjectId.GenerateNewId(), "Open", "Open status", DateTime.UtcNow, null, false, UserDto.Empty),
		false,
		UserDto.Empty,
		false,
		false);

	[Fact]
	public void IssuesPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Issues");
	}

	[Fact]
	public void IssuesPage_HasNewIssueLink()
	{
		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		var link = cut.Find("a[href='/issues/create']");
		link.Should().NotBeNull();
		link.TextContent.Trim().Should().Contain("New Issue");
	}

	[Fact]
	public void IssuesPage_HasSearchInput()
	{
		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		cut.Find("#search").Should().NotBeNull();
	}

	[Fact]
	public void IssuesPage_HasStatusAndCategoryFilters()
	{
		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		cut.Find("#status-filter").Should().NotBeNull();
		cut.Find("#category-filter").Should().NotBeNull();
	}

	[Fact]
	public void IssuesPage_ShowsEmptyMessage_WhenApiReturnsNoIssues()
	{
		// Arrange — mock already returns PaginatedResponse.Empty by default

		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		cut.Markup.Should().Contain("No issues found.");
	}

	[Fact]
	public void IssuesPage_ShowsIssueTitle_WhenApiReturnsData()
	{
		// Arrange
		var issue = MakeIssue("My Integration Bug");
		var response = new PaginatedResponse<IssueDto>(
				[issue],
			1,
			1,
			20);
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(response));

		// Act
		var cut = TestContext.Render<IssuesPage>();

		// Assert
		cut.Markup.Should().Contain("My Integration Bug");
	}

	[Fact]
	public void IssuesPage_CallsGetAllAsync_OnInitialization()
	{
		// Act
		TestContext.Render<IssuesPage>();

		// Assert
		_mockIssueClient.Received(1)
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task IssuesPage_ClearFilters_CallsGetAllAsync_WithPage1()
	{
		// Arrange
		var cut = TestContext.Render<IssuesPage>();

		// Act
		await cut.FindAll("button").First(b => b.TextContent.Trim() == "Clear").ClickAsync(new MouseEventArgs());

		// Assert — once on init, once on clear
		_ = _mockIssueClient.Received(2)
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
	}
}
