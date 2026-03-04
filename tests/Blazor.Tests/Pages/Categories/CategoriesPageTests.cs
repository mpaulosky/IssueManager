// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoriesPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

namespace BlazorTests.Pages.Categories;

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
	public void CategoriesPage_HasNewCategoryLink()
	{
		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		var link = cut.Find("a[href='/categories/create']");
		link.Should().NotBeNull();
		link.TextContent.Trim().Should().Contain("New Category");
	}

	[Fact]
	public void CategoriesPage_ShowsEmptyMessage_WhenApiReturnsNoCategories()
	{
		// Arrange — mock already returns empty by default

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		cut.Markup.Should().Contain("No categories found.");
	}

	[Fact]
	public void CategoriesPage_ShowsCategoryName_WhenApiReturnsData()
	{
		// Arrange
		var category = MakeCategory("Performance");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([ category ]));

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		cut.Markup.Should().Contain("Performance");
	}

	[Fact]
	public void CategoriesPage_ShowsEditLink_ForEachCategory()
	{
		// Arrange
		var category = MakeCategory("Security");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([ category ]));

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		var editLink = cut.Find($"a[href='/categories/{category.Id}/edit']");
		editLink.Should().NotBeNull();
	}

	[Fact]
	public void CategoriesPage_ShowsDeleteButton_ForEachCategory()
	{
		// Arrange
		var category = MakeCategory("UI");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([ category ]));

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		var deleteButtons = cut.FindAll("button").Where(b => b.TextContent.Trim() == "Delete");
		deleteButtons.Should().NotBeEmpty();
	}

	[Fact]
	public void CategoriesPage_CallsGetAllAsync_OnInitialization()
	{
		// Act
		_ctx.Render<CategoriesPage>();

		// Assert
		_mockCategoryClient.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}
}
