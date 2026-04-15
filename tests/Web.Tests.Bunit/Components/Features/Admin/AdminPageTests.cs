// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AdminPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

using Web.Components.Features.Issues;

namespace Web.Components.Features.Admin;

/// <summary>
/// bUnit tests for the <see cref="AdminPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class AdminPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly IIssueApiClient _mockIssueClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="AdminPageTests"/> class.
	/// </summary>
	public AdminPageTests()
	{
		_ctx = new BunitContext();
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");

		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(PaginatedResponse<IssueDto>.Empty));
		_ctx.Services.AddSingleton(_mockIssueClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	private static IssueDto MakePendingIssue(string title = "Pending Issue", string description = "Pending description") => new(
		ObjectId.GenerateNewId(),
		title,
		description,
		DateTime.UtcNow,
		null,
		new UserDto("user1", "Author Name", "author@test.com"),
		new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug category", DateTime.UtcNow, null, false, UserDto.Empty),
		new StatusDto(ObjectId.GenerateNewId(), "Open", "Open status", DateTime.UtcNow, null, false, UserDto.Empty),
		false,
		UserDto.Empty,
		false,
		false);

	private static PaginatedResponse<IssueDto> MakeResponse(params IssueDto[] issues) =>
		new(issues, issues.Length, 1, 100);

	// ─── Initialization ──────────────────────────────────────────────────────────

	[Fact]
	public void AdminPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Pending Issues");
	}

	[Fact]
	public void AdminPage_CallsGetAllAsync_WithPageSize100_OnInitialization()
	{
		// Act
		_ctx.Render<AdminPage>();

		// Assert
		_mockIssueClient.Received(1)
			.GetAllAsync(Arg.Any<int>(), 100, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public void AdminPage_ShowsEmptyMessage_WhenApiReturnsNoPendingIssues()
	{
		// Arrange — mock already returns empty by default

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Markup.Should().Contain("No pending issues to review.");
	}

	[Fact]
	public void AdminPage_ShowsIssueTitle_WhenApiReturnsPendingIssues()
	{
		// Arrange
		var issue = MakePendingIssue("My Pending Bug");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Markup.Should().Contain("My Pending Bug");
	}

	[Fact]
	public void AdminPage_ShowsAuthorAndCategoryInfo_ForPendingIssue()
	{
		// Arrange
		var issue = MakePendingIssue("Info Issue");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Markup.Should().Contain("Author Name");
		cut.Markup.Should().Contain("Bug");
	}

	// ─── Filtering ────────────────────────────────────────────────────────────────

	[Fact]
	public void AdminPage_DoesNotShowApprovedIssue_AndShowsEmptyMessage()
	{
		// Arrange
		var approvedIssue = MakePendingIssue("Approved Issue") with { ApprovedForRelease = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(approvedIssue)));

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Markup.Should().NotContain("Approved Issue");
		cut.Markup.Should().Contain("No pending issues to review.");
	}

	[Fact]
	public void AdminPage_DoesNotShowRejectedIssue_AndShowsEmptyMessage()
	{
		// Arrange
		var rejectedIssue = MakePendingIssue("Rejected Issue") with { Rejected = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(rejectedIssue)));

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Markup.Should().NotContain("Rejected Issue");
		cut.Markup.Should().Contain("No pending issues to review.");
	}

	[Fact]
	public void AdminPage_ShowsApproveAndRejectButtons_ForEachPendingIssue()
	{
		// Arrange
		var issue1 = MakePendingIssue("Issue One");
		var issue2 = MakePendingIssue("Issue Two");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue1, issue2)));

		// Act
		var cut = _ctx.Render<AdminPage>();

		// Assert
		cut.Find($"#approve-{issue1.Id}").Should().NotBeNull();
		cut.Find($"#reject-{issue1.Id}").Should().NotBeNull();
		cut.Find($"#approve-{issue2.Id}").Should().NotBeNull();
		cut.Find($"#reject-{issue2.Id}").Should().NotBeNull();
	}

	// ─── Approve ─────────────────────────────────────────────────────────────────

	[Fact]
	public async Task AdminPage_ApproveIssue_CallsUpdateAsync_WithApprovedForReleaseTrue()
	{
		// Arrange
		var issue = MakePendingIssue("Issue To Approve");
		var updatedIssue = issue with { ApprovedForRelease = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#approve-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockIssueClient.Received(1)
			.UpdateAsync(
				issue.Id.ToString(),
				Arg.Is<UpdateIssueCommand>(c => c.ApprovedForRelease == true && c.Rejected == false),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AdminPage_ApproveIssue_RemovesIssueFromList_WhenUpdateSucceeds()
	{
		// Arrange
		var issue = MakePendingIssue("Issue To Approve");
		var updatedIssue = issue with { ApprovedForRelease = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#approve-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		cut.Markup.Should().Contain("No pending issues to review.");
	}

	[Fact]
	public async Task AdminPage_ApproveIssue_DoesNotRemoveIssue_WhenUpdateReturnsNull()
	{
		// Arrange
		var issue = MakePendingIssue("Issue That Stays");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#approve-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert — issue remains because update failed
		cut.Markup.Should().Contain("Issue That Stays");
	}

	// ─── Reject ──────────────────────────────────────────────────────────────────

	[Fact]
	public async Task AdminPage_RejectIssue_CallsUpdateAsync_WithRejectedTrue()
	{
		// Arrange
		var issue = MakePendingIssue("Issue To Reject");
		var updatedIssue = issue with { Rejected = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#reject-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		await _mockIssueClient.Received(1)
			.UpdateAsync(
				issue.Id.ToString(),
				Arg.Is<UpdateIssueCommand>(c => c.Rejected == true),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AdminPage_RejectIssue_RemovesIssueFromList_WhenUpdateSucceeds()
	{
		// Arrange
		var issue = MakePendingIssue("Issue To Reject");
		var updatedIssue = issue with { Rejected = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#reject-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		cut.Markup.Should().Contain("No pending issues to review.");
	}

	[Fact]
	public async Task AdminPage_RejectIssue_DoesNotRemoveIssue_WhenUpdateReturnsNull()
	{
		// Arrange
		var issue = MakePendingIssue("Rejection Fails");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));

		var cut = _ctx.Render<AdminPage>();

		// Act
		await cut.Find($"#reject-{issue.Id}").ClickAsync(new MouseEventArgs());

		// Assert — issue remains because update failed
		cut.Markup.Should().Contain("Rejection Fails");
	}

	// ─── Edit Title ───────────────────────────────────────────────────────────────

	[Fact]
	public void AdminPage_EditTitle_ShowsTitleInput_WhenEditButtonClicked()
	{
		// Arrange
		var issue = MakePendingIssue("Title To Edit");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();

		// Act — click the first ✎ button (edit title)
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Assert
		cut.FindAll("input[type='text']").Should().HaveCount(1);
	}

	[Fact]
	public void AdminPage_EditTitle_ClearsDescriptionEditState_WhenTitleEditStarts()
	{
		// Arrange
		var issue = MakePendingIssue("Dual Edit Issue");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();

		// Enter description edit mode first (last ✎ button is description)
		cut.FindAll("button").Last(b => b.TextContent.Contains("✎")).Click();

		// Act — now click the title ✎ (only remaining ✎ is the title one)
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Assert — only one input visible (title), description is back to read-only
		cut.FindAll("input[type='text']").Should().HaveCount(1);
	}

	[Fact]
	public async Task AdminPage_SaveTitle_CallsUpdateAsync_WithNewTitle()
	{
		// Arrange
		var issue = MakePendingIssue("Original Title");
		var updatedIssue = issue with { Title = "Updated Title" };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Act — type a new title and save
		await cut.Find("input[type='text']").ChangeAsync("Updated Title");
		await cut.FindAll("button").First(b => b.TextContent.Contains("✓")).ClickAsync(new MouseEventArgs());

		// Assert
		await _mockIssueClient.Received(1)
			.UpdateAsync(
				issue.Id.ToString(),
				Arg.Is<UpdateIssueCommand>(c => c.Title == "Updated Title"),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AdminPage_SaveTitle_HidesTitleInput_AfterSave()
	{
		// Arrange
		var issue = MakePendingIssue("Title To Save");
		var updatedIssue = issue with { Title = "Saved Title" };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();
		await cut.Find("input[type='text']").ChangeAsync("Saved Title");

		// Act
		await cut.FindAll("button").First(b => b.TextContent.Contains("✓")).ClickAsync(new MouseEventArgs());

		// Assert — edit mode is cleared
		cut.FindAll("input[type='text']").Should().BeEmpty();
	}

	[Fact]
	public void AdminPage_CancelEditTitle_HidesTitleInput()
	{
		// Arrange
		var issue = MakePendingIssue("Title To Cancel");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Act — click the cancel (✕) button
		cut.FindAll("button").First(b => b.TextContent.Contains("✕")).Click();

		// Assert
		cut.FindAll("input[type='text']").Should().BeEmpty();
	}

	// ─── Edit Description ─────────────────────────────────────────────────────────

	[Fact]
	public void AdminPage_EditDescription_ShowsDescriptionInput_WhenEditButtonClicked()
	{
		// Arrange
		var issue = MakePendingIssue("Issue", "Description To Edit");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();

		// Act — click the last ✎ button (edit description)
		cut.FindAll("button").Last(b => b.TextContent.Contains("✎")).Click();

		// Assert
		cut.FindAll("input[type='text']").Should().HaveCount(1);
	}

	[Fact]
	public void AdminPage_EditDescription_ClearsTitleEditState_WhenDescriptionEditStarts()
	{
		// Arrange
		var issue = MakePendingIssue("Clear Title On Desc Edit");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();

		// Enter title edit mode first
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Act — now click description ✎ (which is now the only ✎ visible)
		cut.FindAll("button").First(b => b.TextContent.Contains("✎")).Click();

		// Assert — only one input visible (description), title is back to read-only
		cut.FindAll("input[type='text']").Should().HaveCount(1);
	}

	[Fact]
	public async Task AdminPage_SaveDescription_CallsUpdateAsync_WithNewDescription()
	{
		// Arrange
		var issue = MakePendingIssue("Issue", "Original Description");
		var updatedIssue = issue with { Description = "Updated Description" };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(issue.Id.ToString(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").Last(b => b.TextContent.Contains("✎")).Click();

		// Act — type a new description and save
		await cut.Find("input[type='text']").ChangeAsync("Updated Description");
		await cut.FindAll("button").First(b => b.TextContent.Contains("✓")).ClickAsync(new MouseEventArgs());

		// Assert
		await _mockIssueClient.Received(1)
			.UpdateAsync(
				issue.Id.ToString(),
				Arg.Is<UpdateIssueCommand>(c => c.Description == "Updated Description"),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AdminPage_SaveDescription_HidesDescriptionInput_AfterSave()
	{
		// Arrange
		var issue = MakePendingIssue("Issue", "Desc To Save");
		var updatedIssue = issue with { Description = "Saved Description" };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));
		_mockIssueClient
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(updatedIssue));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").Last(b => b.TextContent.Contains("✎")).Click();
		await cut.Find("input[type='text']").ChangeAsync("Saved Description");

		// Act
		await cut.FindAll("button").First(b => b.TextContent.Contains("✓")).ClickAsync(new MouseEventArgs());

		// Assert — edit mode is cleared
		cut.FindAll("input[type='text']").Should().BeEmpty();
	}

	[Fact]
	public void AdminPage_CancelEditDescription_HidesDescriptionInput()
	{
		// Arrange
		var issue = MakePendingIssue("Issue", "Description To Cancel");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		var cut = _ctx.Render<AdminPage>();
		cut.FindAll("button").Last(b => b.TextContent.Contains("✎")).Click();

		// Act — click the cancel (✕) button
		cut.FindAll("button").First(b => b.TextContent.Contains("✕")).Click();

		// Assert
		cut.FindAll("input[type='text']").Should().BeEmpty();
	}
}
