// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Components;

using MongoDB.Bson;

using Shared.DTOs;
using Shared.Validators;

using Web.Pages.Statuses;
using Web.Services;

namespace Tests.BlazorTests.Pages.Statuses;

/// <summary>
/// bUnit tests for the <see cref="EditStatusPage"/> Blazor page.
/// </summary>
public class EditStatusPageTests : IDisposable
{
	private readonly Bunit.TestContext _ctx;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="EditStatusPageTests"/> class.
	/// </summary>
	public EditStatusPageTests()
	{
		_ctx = new Bunit.TestContext();
		_mockStatusClient = Substitute.For<IStatusApiClient>();
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
		_mockStatusClient.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
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
	public void EditStatusPage_ShowsNotFoundMessage_WhenStatusIsNull()
	{
		// Arrange — mock returns null by default

		// Act
		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, "nonexistent-id"));

		// Assert
		cut.Markup.Should().Contain("Status not found.");
	}

	[Fact]
	public void EditStatusPage_ShowsEditHeading_WhenStatusLoads()
	{
		// Arrange
		var status = MakeStatus("In Progress");
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(status));

		// Act
		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, status.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Edit Status");
	}

	[Fact]
	public void EditStatusPage_PrePopulatesNameInput_WhenStatusLoads()
	{
		// Arrange
		var status = MakeStatus("Resolved");
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(status));

		// Act
		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, status.Id.ToString()));

		// Assert
		var nameInput = cut.Find("#status-name");
		nameInput.GetAttribute("value").Should().Be("Resolved");
	}

	[Fact]
	public void EditStatusPage_HasSubmitButton_WithUpdateText()
	{
		// Arrange
		var status = MakeStatus("Closed");
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(status));

		// Act
		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, status.Id.ToString()));

		// Assert
		cut.Find("button[type='submit']").TextContent.Should().Contain("Update Status");
	}

	[Fact]
	public async Task EditStatusPage_SubmitForm_CallsUpdateAsync()
	{
		// Arrange
		var status = MakeStatus("Pending");
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(status));

		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, status.Id.ToString()));

		// Act — change the name and submit
		cut.Find("#status-name").Change("Updated Pending");
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockStatusClient.Received(1)
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditStatusPage_AfterSuccessfulUpdate_NavigatesToStatusesList()
	{
		// Arrange
		var status = MakeStatus("Wontfix");
		_mockStatusClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(status));

		var cut = _ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, status.Id.ToString()));

		cut.Find("#status-name").Change("Updated Wontfix");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = _ctx.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/statuses");
	}

	[Fact]
	public void EditStatusPage_CallsGetByIdAsync_WithCorrectId()
	{
		// Arrange
		const string testId = "status-test-123";

		// Act
		_ctx.Render<EditStatusPage>(p =>
			p.Add(c => c.Id, testId));

		// Assert
		_mockStatusClient.Received(1)
			.GetByIdAsync(testId, Arg.Any<CancellationToken>());
	}
}
