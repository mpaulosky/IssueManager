// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusesPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Statuses;

/// <summary>
/// bUnit tests for the <see cref="StatusesPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusesPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatusesPageTests"/> class.
	/// </summary>
	public StatusesPageTests()
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

	[Fact]
	public void StatusesPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<StatusesPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Statuses");
	}

	[Fact]
	public void StatusesPage_ShowsEmptyMessage_WhenApiReturnsNoStatuses()
	{
		// Arrange — mock already returns empty by default

		// Act
		var cut = _ctx.Render<StatusesPage>();

		// Assert — Radzen DataGrid shows "No records to display." when empty
		cut.Markup.Should().Contain("No records to display.");
	}

	[Fact]
	public void StatusesPage_ShowsStatusName_WhenApiReturnsData()
	{
		// Arrange
		var status = MakeStatus("In Progress");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([ status ]));

		// Act
		var cut = _ctx.Render<StatusesPage>();

		// Assert
		cut.Markup.Should().Contain("In Progress");
	}

	[Fact]
	public void StatusesPage_CallsGetAllAsync_OnInitialization()
	{
		// Act
		_ctx.Render<StatusesPage>();

		// Assert
		_mockStatusClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}
}
