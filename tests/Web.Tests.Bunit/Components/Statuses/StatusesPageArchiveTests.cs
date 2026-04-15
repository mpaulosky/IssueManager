// =============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusesPageArchiveTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Statuses;

/// <summary>
/// bUnit tests verifying archive action behavior in <see cref="StatusesPage"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusesPageArchiveTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatusesPageArchiveTests"/> class.
	/// </summary>
	public StatusesPageArchiveTests()
	{
		_ctx = new BunitContext();
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		_mockStatusClient = Substitute.For<IStatusApiClient>();
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([]));
		_ctx.Services.AddSingleton(_mockStatusClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	private static StatusDto MakeStatus(string name = "Open", string description = "Issue is open") => new(
		ObjectId.GenerateNewId(),
		name,
		description,
		DateTime.UtcNow,
		null,
		false,
		UserDto.Empty);

	// ─── Admin Guard ─────────────────────────────────────────────────────────────

	[Fact]
	public void ArchiveButton_AdminUser_IsVisible()
	{
		// Arrange
		var status = MakeStatus("In Progress");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");

		// Act
		var cut = _ctx.Render<StatusesPage>();

		// Assert
		cut.Find($"#archive-{status.Id}").Should().NotBeNull();
	}

	[Fact]
	public void ArchiveButton_NonAdminUser_IsNotVisible()
	{
		// Arrange
		var status = MakeStatus("In Progress");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_ctx.AddAuthorization().SetAuthorized("Regular User");

		// Act
		var cut = _ctx.Render<StatusesPage>();

		// Assert — archive button must not be present for non-admin users
		cut.FindAll($"#archive-{status.Id}").Should().BeEmpty();
	}

	// ─── Confirmation Dialog ──────────────────────────────────────────────────────

	[Fact]
	public async Task ArchiveButton_Clicked_ShowsConfirmDialog()
	{
		// Arrange
		var status = MakeStatus("Watching");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<StatusesPage>();

		// Act
		await cut.Find($"#archive-{status.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		cut.Find("[role='dialog']").Should().NotBeNull();
	}

	[Fact]
	public async Task ConfirmDialog_Confirmed_CallsArchiveApiAndRemovesRow()
	{
		// Arrange
		var status = MakeStatus("Dismissed");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_mockStatusClient.ArchiveAsync(status.Id.ToString(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(true));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<StatusesPage>();

		// Open dialog
		await cut.Find($"#archive-{status.Id}").ClickAsync(new MouseEventArgs());

		// Act — click the confirm button
		var confirmButton = cut.FindAll("button").First(b => b.TextContent.Contains("Yes, Archive"));
		await confirmButton.ClickAsync(new MouseEventArgs());

		// Assert
		await _mockStatusClient.Received(1).ArchiveAsync(status.Id.ToString(), Arg.Any<CancellationToken>());
		cut.Markup.Should().NotContain("Dismissed");
	}

	[Fact]
	public async Task ConfirmDialog_Cancelled_DoesNotCallApi()
	{
		// Arrange
		var status = MakeStatus("Keep Me");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<StatusesPage>();

		// Open dialog
		await cut.Find($"#archive-{status.Id}").ClickAsync(new MouseEventArgs());

		// Act — click the cancel button
		var cancelButton = cut.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
		await cancelButton.ClickAsync(new MouseEventArgs());

		// Assert — API must not have been called and dialog must be hidden
		await _mockStatusClient.DidNotReceive().ArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		cut.FindAll("[role='dialog']").Should().BeEmpty();
	}

	// ─── Error Handling ───────────────────────────────────────────────────────────

	[Fact]
	public async Task ArchiveApi_ReturnsError_ShowsErrorMessage()
	{
		// Arrange
		var status = MakeStatus("Failing");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));
		_mockStatusClient.ArchiveAsync(status.Id.ToString(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(false));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<StatusesPage>();

		// Open dialog and confirm
		await cut.Find($"#archive-{status.Id}").ClickAsync(new MouseEventArgs());
		var confirmButton = cut.FindAll("button").First(b => b.TextContent.Contains("Yes, Archive"));
		await confirmButton.ClickAsync(new MouseEventArgs());

		// Assert — error element is shown
		cut.Find("#archive-error").Should().NotBeNull();
		cut.Find("#archive-error").TextContent.Should().NotBeNullOrWhiteSpace();
	}
}
