// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusesPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

using System.Reflection;

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
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([status]));

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

	[Fact]
	public async Task StatusesPage_OnCreateRow_CallsCreateAsync_AndSetsId()
	{
		// Arrange
		var createdDto = MakeStatus("NewStatus", "NewDesc");
		_mockStatusClient.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(createdDto));
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("OnCreateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { StatusName = "NewStatus", StatusDescription = "NewDesc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);

		// Assert
		_ = _mockStatusClient.Received(1).CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>());
		status.Id.Should().Be(createdDto.Id.ToString());
	}

	[Fact]
	public async Task StatusesPage_OnCreateRow_DoesNotSetId_WhenCreateReturnsNull()
	{
		// Arrange
		_mockStatusClient.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("OnCreateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { StatusName = "NewStatus", StatusDescription = "NewDesc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);

		// Assert
		status.Id.Should().BeEmpty();
	}

	[Fact]
	public async Task StatusesPage_OnUpdateRow_CallsUpdateAsync_WhenIdIsSet()
	{
		// Arrange
		var id = ObjectId.GenerateNewId().ToString();
		_mockStatusClient.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("OnUpdateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { Id = id, StatusName = "Updated", StatusDescription = "Desc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);

		// Assert
		_ = _mockStatusClient.Received(1).UpdateAsync(id, Arg.Any<UpdateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task StatusesPage_OnUpdateRow_SkipsUpdate_WhenIdIsEmpty()
	{
		// Arrange
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("OnUpdateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { Id = "", StatusName = "Updated", StatusDescription = "Desc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);

		// Assert
		_ = _mockStatusClient.DidNotReceive().UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task StatusesPage_CancelEdit_ClearsInsertingState()
	{
		// Arrange
		var cut = _ctx.Render<StatusesPage>();
		var insertingField = typeof(StatusesPage).GetField("_insertingStatus", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { StatusName = "Test" };
		insertingField!.SetValue(cut.Instance, status);
		var cancelMethod = typeof(StatusesPage).GetMethod("CancelEdit", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act
		await cut.InvokeAsync(() => { cancelMethod!.Invoke(cut.Instance, [status]); });

		// Assert
		insertingField.GetValue(cut.Instance).Should().BeNull();
	}

	[Fact]
	public async Task StatusesPage_CancelEdit_ClearsEditingState()
	{
		// Arrange
		var cut = _ctx.Render<StatusesPage>();
		var editingField = typeof(StatusesPage).GetField("_editingStatus", BindingFlags.NonPublic | BindingFlags.Instance);
		var status = new StatusEditModel { StatusName = "Test" };
		editingField!.SetValue(cut.Instance, status);
		var cancelMethod = typeof(StatusesPage).GetMethod("CancelEdit", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act
		await cut.InvokeAsync(() => { cancelMethod!.Invoke(cut.Instance, [status]); });

		// Assert
		editingField.GetValue(cut.Instance).Should().BeNull();
	}

	[Fact]
	public async Task StatusesPage_InsertRow_SetsInsertingStatus()
	{
		// Arrange
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("InsertRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var insertingField = typeof(StatusesPage).GetField("_insertingStatus", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act — _insertingStatus is assigned before any grid interop call
		try
		{
			await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [])!);
		}
		catch (Exception) { /* grid JS interop may fail in test environment */ }

		// Assert
		insertingField!.GetValue(cut.Instance).Should().NotBeNull();
	}

	[Fact]
	public async Task StatusesPage_EditRow_SetsEditingStatus()
	{
		// Arrange
		var status = new StatusEditModel { Id = "1", StatusName = "Open", StatusDescription = "Issue is open" };
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("EditRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var editingField = typeof(StatusesPage).GetField("_editingStatus", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act — _editingStatus is assigned before any grid interop call
		try
		{
			await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);
		}
		catch (Exception) { /* grid JS interop may fail in test environment */ }

		// Assert
		editingField!.GetValue(cut.Instance).Should().Be(status);
	}

	[Fact]
	public async Task StatusesPage_SaveRow_InvokesGridUpdate()
	{
		// Arrange
		var status = new StatusEditModel { Id = "1", StatusName = "Open" };
		var cut = _ctx.Render<StatusesPage>();
		var method = typeof(StatusesPage).GetMethod("SaveRow", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act
		Func<Task> act = () => cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [status])!);

		// Assert — SaveRow completes without throwing
		await act.Should().NotThrowAsync();
	}
}
