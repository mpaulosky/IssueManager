// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoriesPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

using System.Reflection;

namespace Web.Components.Features.Categories;

/// <summary>
/// bUnit tests for the <see cref="CategoriesPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoriesPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly ICategoryApiClient _mockCategoryClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="CategoriesPageTests"/> class.
	/// </summary>
	public CategoriesPageTests()
	{
		_ctx = new BunitContext();
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		_mockCategoryClient = Substitute.For<ICategoryApiClient>();
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([]));
		_ctx.Services.AddSingleton(_mockCategoryClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	private static CategoryDto MakeCategory(string name = "Bug", string description = "Bug fixes") => new(
		ObjectId.GenerateNewId(),
		name,
		description,
		DateTime.UtcNow,
		null,
		false,
		UserDto.Empty);

	[Fact]
	public void CategoriesPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Categories");
	}

	[Fact]
	public void CategoriesPage_ShowsEmptyMessage_WhenApiReturnsNoCategories()
	{
		// Arrange — mock already returns empty by default

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert — Radzen DataGrid shows "No records to display." when empty
		cut.Markup.Should().Contain("No records to display.");
	}

	[Fact]
	public void CategoriesPage_ShowsCategoryName_WhenApiReturnsData()
	{
		// Arrange
		var category = MakeCategory("Performance");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		cut.Markup.Should().Contain("Performance");
	}

	[Fact]
	public void CategoriesPage_CallsGetAllAsync_OnInitialization()
	{
		// Act
		_ctx.Render<CategoriesPage>();

		// Assert
		_mockCategoryClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CategoriesPage_OnCreateRow_CallsCreateAsync_AndSetsId()
	{
		// Arrange
		var createdDto = MakeCategory("NewCat", "NewDesc");
		_mockCategoryClient.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(createdDto));
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("OnCreateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { CategoryName = "NewCat", CategoryDescription = "NewDesc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);

		// Assert
		_ = _mockCategoryClient.Received(1).CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
		cat.Id.Should().Be(createdDto.Id.ToString());
	}

	[Fact]
	public async Task CategoriesPage_OnCreateRow_DoesNotSetId_WhenCreateReturnsNull()
	{
		// Arrange
		_mockCategoryClient.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("OnCreateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { CategoryName = "NewCat", CategoryDescription = "NewDesc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);

		// Assert
		cat.Id.Should().BeEmpty();
	}

	[Fact]
	public async Task CategoriesPage_OnUpdateRow_CallsUpdateAsync_WhenIdIsSet()
	{
		// Arrange
		var id = ObjectId.GenerateNewId().ToString();
		_mockCategoryClient.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("OnUpdateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { Id = id, CategoryName = "Updated", CategoryDescription = "Desc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);

		// Assert
		_ = _mockCategoryClient.Received(1).UpdateAsync(id, Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CategoriesPage_OnUpdateRow_SkipsUpdate_WhenIdIsEmpty()
	{
		// Arrange
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("OnUpdateRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { Id = "", CategoryName = "Updated", CategoryDescription = "Desc" };

		// Act
		await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);

		// Assert
		_ = _mockCategoryClient.DidNotReceive().UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CategoriesPage_CancelEdit_ClearsInsertingState()
	{
		// Arrange
		var cut = _ctx.Render<CategoriesPage>();
		var insertingField = typeof(CategoriesPage).GetField("_insertingCategory", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { CategoryName = "Test" };
		insertingField!.SetValue(cut.Instance, cat);
		var cancelMethod = typeof(CategoriesPage).GetMethod("CancelEdit", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act
		await cut.InvokeAsync(() => { cancelMethod!.Invoke(cut.Instance, [cat]); });

		// Assert
		insertingField.GetValue(cut.Instance).Should().BeNull();
	}

	[Fact]
	public async Task CategoriesPage_CancelEdit_ClearsEditingState()
	{
		// Arrange
		var cut = _ctx.Render<CategoriesPage>();
		var editingField = typeof(CategoriesPage).GetField("_editingCategory", BindingFlags.NonPublic | BindingFlags.Instance);
		var cat = new CategoryEditModel { CategoryName = "Test" };
		editingField!.SetValue(cut.Instance, cat);
		var cancelMethod = typeof(CategoriesPage).GetMethod("CancelEdit", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act
		await cut.InvokeAsync(() => { cancelMethod!.Invoke(cut.Instance, [cat]); });

		// Assert
		editingField.GetValue(cut.Instance).Should().BeNull();
	}

	[Fact]
	public async Task CategoriesPage_InsertRow_SetsInsertingCategory()
	{
		// Arrange
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("InsertRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var insertingField = typeof(CategoriesPage).GetField("_insertingCategory", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act — _insertingCategory is assigned before any grid interop call
		try
		{
			await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [])!);
		}
		catch (Exception) { /* grid JS interop may fail in test environment */ }

		// Assert
		insertingField!.GetValue(cut.Instance).Should().NotBeNull();
	}

	[Fact]
	public async Task CategoriesPage_EditRow_SetsEditingCategory()
	{
		// Arrange
		var cat = new CategoryEditModel { Id = "1", CategoryName = "Bug", CategoryDescription = "Bugs" };
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("EditRow", BindingFlags.NonPublic | BindingFlags.Instance);
		var editingField = typeof(CategoriesPage).GetField("_editingCategory", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act — _editingCategory is assigned before any grid interop call
		try
		{
			await cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);
		}
		catch (Exception) { /* grid JS interop may fail in test environment */ }

		// Assert
		editingField!.GetValue(cut.Instance).Should().Be(cat);
	}

	[Fact]
	public async Task CategoriesPage_SaveRow_InvokesGridUpdate()
	{
		// Arrange
		var cat = new CategoryEditModel { Id = "1", CategoryName = "Bug" };
		var cut = _ctx.Render<CategoriesPage>();
		var method = typeof(CategoriesPage).GetMethod("SaveRow", BindingFlags.NonPublic | BindingFlags.Instance);

		// Act & Assert — SaveRow completes without throwing (Radzen grid interop handled by Loose JS mode)
		Func<Task> act = () => cut.InvokeAsync(() => (Task)method!.Invoke(cut.Instance, [cat])!);

		await act.Should().NotThrowAsync();
	}
}
