// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueDetailPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Issues;

/// <summary>
/// bUnit tests for the <see cref="IssueDetailPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class IssueDetailPageTests : ComponentTestBase
{
	private readonly IIssueApiClient _mockIssueClient;
	private readonly ICommentApiClient _mockCommentClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="IssueDetailPageTests"/> class.
	/// </summary>
	public IssueDetailPageTests()
	{
		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));
		TestContext.Services.AddSingleton(_mockIssueClient);

		_mockCommentClient = Substitute.For<ICommentApiClient>();
		_mockCommentClient
			.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([]));
		TestContext.Services.AddSingleton(_mockCommentClient);
	}

	private static IssueDto MakeIssue(string title = "Detail Issue", string id = "") => new(
		string.IsNullOrEmpty(id) ? ObjectId.GenerateNewId() : ObjectId.Parse(id),
		title,
		"Issue description text",
		DateTime.UtcNow,
		null,
		new UserDto("user1", "Author Name", "author@test.com"),
		new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug category", DateTime.UtcNow, null, false, UserDto.Empty),
		new StatusDto(ObjectId.GenerateNewId(), "Open", "Open status", DateTime.UtcNow, null, false, UserDto.Empty),
		false,
		UserDto.Empty,
		false,
		false);

	private static CommentDto MakeComment(IssueDto issue) => new(
		ObjectId.GenerateNewId(),
		"Comment",
		"Test comment text",
		DateTime.UtcNow,
		null,
		issue,
		new UserDto("user2", "Commenter", "commenter@test.com"),
		[],
		false,
		UserDto.Empty,
		false,
		UserDto.Empty);

	[Fact]
	public void IssueDetailPage_ShowsNotFoundMessage_WhenIssueIsNull()
	{
		// Arrange — mock returns null by default

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, "nonexistent-id"));

		// Assert
		cut.Markup.Should().Contain("Issue not found.");
	}

	[Fact]
	public void IssueDetailPage_ShowsIssueTitle_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue("My Loaded Issue");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("My Loaded Issue");
	}

	[Fact]
	public void IssueDetailPage_ShowsAuthorName_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue("Some Issue");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Author Name");
	}

	[Fact]
	public void IssueDetailPage_ShowsCommentsSection_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue("Issue With Comments");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Comments");
		cut.Find("#comment-text").Should().NotBeNull();
	}

	[Fact]
	public void IssueDetailPage_ShowsEmptyCommentsMessage_WhenNoComments()
	{
		// Arrange
		var issue = MakeIssue("Issue No Comments");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		// mock already returns empty comments by default

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("No comments yet.");
	}

	[Fact]
	public void IssueDetailPage_ShowsComment_WhenApiReturnsComments()
	{
		// Arrange
		var issue = MakeIssue("Issue With Real Comments");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([ comment ]));

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Test comment text");
	}

	[Fact]
	public void IssueDetailPage_HasBackToIssuesLink_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue("Issue With Back Link");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Find("a[href='/issues']").Should().NotBeNull();
	}

	[Fact]
	public void IssueDetailPage_CallsGetByIdAsync_WithCorrectId()
	{
		// Arrange
		const string testId = "abc123";

		// Act
		TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, testId));

		// Assert
		_mockIssueClient.Received(1)
			.GetByIdAsync(testId, Arg.Any<CancellationToken>());
	}
}
