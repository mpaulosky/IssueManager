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
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));

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

	[Fact]
	public async Task IssueDetailPage_PostsComment_WhenTextEnteredAndPostButtonClicked()
	{
		// Arrange
		var issue = MakeIssue("Issue For Comment");
		var newComment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.CreateAsync(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CommentDto?>(newComment));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act
		cut.Find("#comment-text").Change("My new comment");
		await cut.FindAll("button").First(b => b.TextContent.Trim() == "Post Comment").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockCommentClient.Received(1).CreateAsync(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void IssueDetailPage_DoesNotPostComment_WhenTextIsEmpty()
	{
		// Arrange
		var issue = MakeIssue("Issue Empty Comment");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act — button is disabled when textarea is empty
		var postButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "Post Comment");

		// Assert
		postButton.GetAttribute("disabled").Should().NotBeNull();
		_mockCommentClient.DidNotReceive().CreateAsync(Arg.Any<CreateCommentCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void IssueDetailPage_ShowsEditMode_WhenEditCommentClicked()
	{
		// Arrange
		var issue = MakeIssue("Issue Edit Comment");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act
		cut.FindAll("button").First(b => b.TextContent.Trim() == "Edit").Click();

		// Assert
		cut.Markup.Should().Contain("Test comment text");
		cut.FindAll("button").Any(b => b.TextContent.Trim() == "Save").Should().BeTrue();
	}

	[Fact]
	public void IssueDetailPage_CancelsEditComment_WhenCancelClicked()
	{
		// Arrange
		var issue = MakeIssue("Issue Cancel Edit");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		cut.FindAll("button").First(b => b.TextContent.Trim() == "Edit").Click();

		// Act
		cut.FindAll("button").First(b => b.TextContent.Trim() == "Cancel").Click();

		// Assert
		cut.FindAll("button").Any(b => b.TextContent.Trim() == "Save").Should().BeFalse();
		cut.Markup.Should().Contain("Test comment text");
	}

	[Fact]
	public async Task IssueDetailPage_SavesEditedComment_WhenSaveClicked()
	{
		// Arrange
		var issue = MakeIssue("Issue Save Edit");
		var comment = MakeComment(issue);
		var updatedComment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));
		_mockCommentClient.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCommentCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CommentDto?>(updatedComment));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		cut.FindAll("button").First(b => b.TextContent.Trim() == "Edit").Click();
		cut.Find("textarea:not(#comment-text)").Change("Updated text");

		// Act
		await cut.FindAll("button").First(b => b.TextContent.Trim() == "Save").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockCommentClient.Received(1).UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCommentCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void IssueDetailPage_ShowsDeleteDialog_WhenDeleteCommentClicked()
	{
		// Arrange
		var issue = MakeIssue("Issue Delete Dialog");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act — click the small red Delete button in the comment row
		cut.Find("button.text-red-600").Click();

		// Assert — the dialog should now be visible
		cut.Markup.Should().Contain("Delete Comment");
		cut.Find("[role=\"dialog\"]").Should().NotBeNull();
	}

	[Fact]
	public void IssueDetailPage_CancelsDelete_WhenDialogCancelled()
	{
		// Arrange
		var issue = MakeIssue("Issue Cancel Delete");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		cut.Find("button.text-red-600").Click();

		// Act — click Cancel in the dialog
		cut.FindAll("button").First(b => b.TextContent.Trim() == "Cancel").Click();

		// Assert
		_mockCommentClient.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task IssueDetailPage_DeletesComment_WhenConfirmDialogConfirmed()
	{
		// Arrange
		var issue = MakeIssue("Issue Confirm Delete");
		var comment = MakeComment(issue);
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockCommentClient.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CommentDto>>([comment]));
		_mockCommentClient.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(true));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		cut.Find("button.text-red-600").Click();

		// Act — click the confirm "Delete" button inside the dialog
		await cut.Find("[role=\"dialog\"] button.bg-\\[var\\(--color-primary\\)\\]").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockCommentClient.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task IssueDetailPage_SetsIssueStatus_WhenAdminClicksStatusButton()
	{
		// Arrange — authorize as Admin
		TestContext.AddAuthorization().SetAuthorized("admin").SetRoles("Admin");

		var issue = MakeIssue("Issue Set Status");
		var status = new StatusDto(ObjectId.GenerateNewId(), "In Progress", "In Progress status", DateTime.UtcNow, null, false, UserDto.Empty);
		var updatedIssue = MakeIssue("Issue Set Status");

		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));
		_mockIssueClient.UpdateStatusAsync(Arg.Any<string>(), Arg.Any<UpdateIssueStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		// Override the base status client mock to return our status
		TestContext.Services.GetRequiredService<IStatusApiClient>()
			.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));

		var cut = TestContext.Render<IssueDetailPage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act
		await cut.FindAll("button").First(b => b.TextContent.Trim() == "In Progress").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockIssueClient.Received(1).UpdateStatusAsync(Arg.Any<string>(), Arg.Any<UpdateIssueStatusCommand>(), Arg.Any<CancellationToken>());
	}
}
