// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     SampleDataPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Admin;

/// <summary>
/// bUnit tests for the <see cref="SampleDataPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class SampleDataPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly ICategoryApiClient _mockCategoryClient;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="SampleDataPageTests"/> class.
	/// </summary>
	public SampleDataPageTests()
	{
		_ctx = new BunitContext();

		var authContext = _ctx.AddAuthorization();
		authContext.SetAuthorized("AdminUser");
		authContext.SetRoles("Admin");

		_mockCategoryClient = Substitute.For<ICategoryApiClient>();
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([]));
		_mockCategoryClient.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
		_ctx.Services.AddSingleton(_mockCategoryClient);

		_mockStatusClient = Substitute.For<IStatusApiClient>();
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([]));
		_mockStatusClient.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
		_ctx.Services.AddSingleton(_mockStatusClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	private static CategoryDto MakeCategory(string name = "Design", string description = "An issue with the design.") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	private static StatusDto MakeStatus(string name = "Answered", string description = "The suggestion was accepted.") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	// ─── Render ─────────────────────────────────────────────────────────────────

	[Fact]
	public void SampleDataPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Sample Data");
	}

	[Fact]
	public void SampleDataPage_ShowsCreateCategoriesButton_WhenNoCategoriesExist()
	{
		// Arrange — mock returns empty by default

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().Contain(b => b.TextContent.Contains("Create Categories"));
	}

	[Fact]
	public void SampleDataPage_ShowsCreateStatusesButton_WhenNoStatusesExist()
	{
		// Arrange — mock returns empty by default

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().Contain(b => b.TextContent.Contains("Create Statuses"));
	}

	// ─── OnInitializedAsync ──────────────────────────────────────────────────────

	[Fact]
	public void SampleDataPage_OnInitializedAsync_CallsGetAllAsync_ForBothClients()
	{
		// Act
		_ctx.Render<SampleDataPage>();

		// Assert
		_mockCategoryClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
		_mockStatusClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public void SampleDataPage_OnInitializedAsync_WhenCategoriesExist_ShowsCategoriesCreatedText()
	{
		// Arrange
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([MakeCategory()]));

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		cut.Markup.Should().Contain("Categories have been created");
	}

	[Fact]
	public void SampleDataPage_OnInitializedAsync_WhenStatusesExist_ShowsStatusesCreatedText()
	{
		// Arrange
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([MakeStatus()]));

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		cut.Markup.Should().Contain("Statuses have been created");
	}

	[Fact]
	public void SampleDataPage_OnInitializedAsync_WhenCategoriesExist_HidesCreateCategoriesButton()
	{
		// Arrange
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([MakeCategory()]));

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().NotContain(b => b.TextContent.Contains("Create Categories"));
	}

	[Fact]
	public void SampleDataPage_OnInitializedAsync_WhenStatusesExist_HidesCreateStatusesButton()
	{
		// Arrange
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([MakeStatus()]));

		// Act
		var cut = _ctx.Render<SampleDataPage>();

		// Assert
		var buttons = cut.FindAll("button");
		buttons.Should().NotContain(b => b.TextContent.Contains("Create Statuses"));
	}

	// ─── CreateCategories ────────────────────────────────────────────────────────

	[Fact]
	public async Task SampleDataPage_CreateCategories_WhenNoneExist_CallsCreateAsync5Times()
	{
		// Arrange
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		await _mockCategoryClient.Received(5)
			.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SampleDataPage_CreateCategories_WhenNoneExist_ShowsCategoriesCreatedText()
	{
		// Arrange
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		cut.Markup.Should().Contain("Categories have been created");
	}

	[Fact]
	public async Task SampleDataPage_CreateCategories_WhenAlreadyExist_DoesNotCallCreateAsync()
	{
		// Arrange — first call (init) returns empty so button renders;
		//           second call (inside CreateCategories) returns data → skips
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(
				Task.FromResult<IEnumerable<CategoryDto>>([]),
				Task.FromResult<IEnumerable<CategoryDto>>([MakeCategory()]));

		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		await _mockCategoryClient.DidNotReceive()
			.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SampleDataPage_CreateCategories_OnException_ShowsErrorMessage()
	{
		// Arrange — second GetAllAsync call (inside CreateCategories) throws
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(
				Task.FromResult<IEnumerable<CategoryDto>>([]),
				Task.FromException<IEnumerable<CategoryDto>>(new InvalidOperationException("Network failure")));

		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		cut.Markup.Should().Contain("Error creating categories");
		cut.Markup.Should().Contain("Network failure");
	}

	[Fact]
	public async Task SampleDataPage_CreateCategories_AfterCompletion_SetsIsWorkingFalse()
	{
		// Arrange
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act
		await createBtn.ClickAsync();

		// Assert — after completion _isWorking = false; the "Create Statuses" button (still visible) must NOT be disabled
		var createStatusBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Create Statuses"));
		createStatusBtn.Should().NotBeNull();
		createStatusBtn!.HasAttribute("disabled").Should().BeFalse();
	}

	// ─── CreateStatuses ──────────────────────────────────────────────────────────

	[Fact]
	public async Task SampleDataPage_CreateStatuses_WhenNoneExist_CallsCreateAsync4Times()
	{
		// Arrange
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Statuses"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		await _mockStatusClient.Received(4)
			.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SampleDataPage_CreateStatuses_WhenNoneExist_ShowsStatusesCreatedText()
	{
		// Arrange
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Statuses"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		cut.Markup.Should().Contain("Statuses have been created");
	}

	[Fact]
	public async Task SampleDataPage_CreateStatuses_WhenAlreadyExist_DoesNotCallCreateAsync()
	{
		// Arrange — first call (init) returns empty so button renders;
		//           second call (inside CreateStatuses) returns data → skips
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(
				Task.FromResult<IEnumerable<StatusDto>>([]),
				Task.FromResult<IEnumerable<StatusDto>>([MakeStatus()]));

		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Statuses"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		await _mockStatusClient.DidNotReceive()
			.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SampleDataPage_CreateStatuses_OnException_ShowsErrorMessage()
	{
		// Arrange — second GetAllAsync call (inside CreateStatuses) throws
		_mockStatusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(
				Task.FromResult<IEnumerable<StatusDto>>([]),
				Task.FromException<IEnumerable<StatusDto>>(new InvalidOperationException("Connection refused")));

		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Statuses"));

		// Act
		await createBtn.ClickAsync();

		// Assert
		cut.Markup.Should().Contain("Error creating statuses");
		cut.Markup.Should().Contain("Connection refused");
	}

	// ─── Idempotency ─────────────────────────────────────────────────────────────

	[Fact]
	public async Task SampleDataPage_CreateCategories_SecondClick_IsNoOp()
	{
		// Arrange — After first click categories are created; button disappears; verify CreateAsync only called 5x total
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Categories"));

		// Act — first click creates all categories
		await createBtn.ClickAsync();

		// Assert — button is gone (categories created) so second invocation cannot happen
		cut.FindAll("button").Should().NotContain(b => b.TextContent.Contains("Create Categories"));
		await _mockCategoryClient.Received(5)
			.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SampleDataPage_CreateStatuses_SecondClick_IsNoOp()
	{
		// Arrange — After first click statuses are created; button disappears; verify CreateAsync only called 4x total
		var cut = _ctx.Render<SampleDataPage>();
		var createBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Create Statuses"));

		// Act — first click creates all statuses
		await createBtn.ClickAsync();

		// Assert — button is gone (statuses created) so second invocation cannot happen
		cut.FindAll("button").Should().NotContain(b => b.TextContent.Contains("Create Statuses"));
		await _mockStatusClient.Received(4)
			.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>());
	}
}
