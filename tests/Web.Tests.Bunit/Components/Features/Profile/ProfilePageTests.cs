// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ProfilePageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

using Web.Components.Features.Issues;

namespace Web.Components.Features.Profile;

/// <summary>
/// bUnit tests for the <see cref="ProfilePage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProfilePageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly IIssueApiClient _mockIssueClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProfilePageTests"/> class.
	/// </summary>
	public ProfilePageTests()
	{
		_ctx = new BunitContext();
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		_ctx.AddAuthorization().SetAuthorized("testuser");

		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(PaginatedResponse<IssueDto>.Empty));
		_ctx.Services.AddSingleton(_mockIssueClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
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

	private static PaginatedResponse<IssueDto> MakeResponse(params IssueDto[] issues) =>
		new(issues, issues.Length, 1, 200);

	// ─── Initialization ──────────────────────────────────────────────────────────

	[Fact]
	public void ProfilePage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Find("h1").TextContent.Should().Contain("testuser");
	}

	[Fact]
	public void ProfilePage_OnInitialized_CallsGetAllAsync_WithPageSize200AndAuthorName()
	{
		// Act
		_ctx.Render<ProfilePage>();

		// Assert
		_mockIssueClient.Received(1)
			.GetAllAsync(Arg.Any<int>(), 200, Arg.Any<string?>(), "testuser", Arg.Any<CancellationToken>());
	}

	// ─── Issue Filtering ─────────────────────────────────────────────────────────

	[Fact]
	public void ProfilePage_ShowsApprovedSection_WhenApiReturnsApprovedIssues()
	{
		// Arrange — approved: ApprovedForRelease=true, Rejected=false, Archived=false
		var issue = MakeIssue("Approved Issue") with { ApprovedForRelease = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert
		cut.Markup.Should().Contain("Approved Issues");
		cut.Markup.Should().Contain("Approved Issue");
	}

	[Fact]
	public void ProfilePage_ShowsPendingSection_WhenApiReturnsPendingIssues()
	{
		// Arrange — pending: ApprovedForRelease=false, Rejected=false, Archived=false (defaults)
		var issue = MakeIssue("Pending Issue");
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert
		cut.Markup.Should().Contain("Pending Issues");
		cut.Markup.Should().Contain("Pending Issue");
	}

	[Fact]
	public void ProfilePage_ShowsRejectedSection_WhenApiReturnsRejectedIssues()
	{
		// Arrange — rejected: Rejected=true
		var issue = MakeIssue("Rejected Issue") with { Rejected = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert
		cut.Markup.Should().Contain("Rejected Issues");
		cut.Markup.Should().Contain("Rejected Issue");
	}

	[Fact]
	public void ProfilePage_ExcludesArchivedIssues_FromAllLists()
	{
		// Arrange — archived: Archived=true, ApprovedForRelease=false, Rejected=false
		var issue = MakeIssue("Archived Issue") with { Archived = true };
		_mockIssueClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(MakeResponse(issue)));

		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert — archived issue falls into no list; empty state is shown
		cut.Markup.Should().NotContain("Approved Issues");
		cut.Markup.Should().NotContain("Pending Issues");
		cut.Markup.Should().NotContain("Rejected Issues");
		cut.Markup.Should().Contain("You have not submitted any issues yet");
	}

	// ─── Loading State ────────────────────────────────────────────────────────────

	[Fact]
	public void ProfilePage_HidesLoadingSpinner_AfterSuccessfulLoad()
	{
		// Act
		var cut = _ctx.Render<ProfilePage>();

		// Assert — OnInitializedAsync completes before Render returns; loading state is gone
		cut.Markup.Should().NotContain("Loading...");
	}

	// ─── Username Fallback ────────────────────────────────────────────────────────

	[Fact]
	public void ProfilePage_ShowsUserFallback_WhenAuthStateHasNoName()
	{
		// Arrange — anonymous user: Identity.Name is null → production code falls back to "User"
		using var ctx = new BunitContext();
		ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		ctx.AddAuthorization(); // no SetAuthorized → anonymous user, Identity.Name = null

		var mockClient = Substitute.For<IIssueApiClient>();
		mockClient
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(PaginatedResponse<IssueDto>.Empty));
		ctx.Services.AddSingleton(mockClient);

		// Act
		var cut = ctx.Render<ProfilePage>();

		// Assert
		cut.Find("h1").TextContent.Should().Contain("User");
	}
}
