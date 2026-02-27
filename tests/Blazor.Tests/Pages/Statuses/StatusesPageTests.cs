// Copyright (c) 2026. All rights reserved.

using MongoDB.Bson;

using Shared.DTOs;

using Web.Pages.Statuses;
using Web.Services;

namespace Tests.BlazorTests.Pages.Statuses;

/// <summary>
/// bUnit tests for the <see cref="StatusesPage"/> Blazor page.
/// </summary>
public class StatusesPageTests : IDisposable
{
	private readonly TestContext _ctx;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatusesPageTests"/> class.
	/// </summary>
	public StatusesPageTests()
	{
		_ctx = new TestContext();
		_mockStatusClient = Substitute.For<IStatusApiClient>();
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([]));
		_ctx.Services.AddSingleton<IStatusApiClient>(_mockStatusClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx?.Dispose();
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
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Statuses");
	}

	[Fact]
	public void StatusesPage_HasNewStatusLink()
	{
		// Act
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		var link = cut.Find("a[href='/statuses/create']");
		link.Should().NotBeNull();
		link.TextContent.Trim().Should().Contain("New Status");
	}

	[Fact]
	public void StatusesPage_ShowsEmptyMessage_WhenApiReturnsNoStatuses()
	{
		// Arrange — mock already returns empty by default

		// Act
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		cut.Markup.Should().Contain("No statuses found.");
	}

	[Fact]
	public void StatusesPage_ShowsStatusName_WhenApiReturnsData()
	{
		// Arrange
		var status = MakeStatus("In Progress");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>(new[] { status }));

		// Act
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		cut.Markup.Should().Contain("In Progress");
	}

	[Fact]
	public void StatusesPage_ShowsEditLink_ForEachStatus()
	{
		// Arrange
		var status = MakeStatus("Resolved");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>(new[] { status }));

		// Act
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		var editLink = cut.Find($"a[href='/statuses/{status.Id}/edit']");
		editLink.Should().NotBeNull();
	}

	[Fact]
	public void StatusesPage_ShowsDeleteButton_ForEachStatus()
	{
		// Arrange
		var status = MakeStatus("Closed");
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>(new[] { status }));

		// Act
		var cut = _ctx.RenderComponent<StatusesPage>();

		// Assert
		var deleteButtons = cut.FindAll("button").Where(b => b.TextContent.Trim() == "Delete");
		deleteButtons.Should().NotBeEmpty();
	}

	[Fact]
	public void StatusesPage_CallsGetAllAsync_OnInitialization()
	{
		// Act
		_ctx.RenderComponent<StatusesPage>();

		// Assert
		_mockStatusClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}
}
