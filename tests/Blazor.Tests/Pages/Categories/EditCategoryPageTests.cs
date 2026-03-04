// Copyright (c) 2026. All rights reserved.

namespace BlazorTests.Pages.Categories;

/// <summary>
/// bUnit tests for the <see cref="EditCategoryPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditCategoryPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly ICategoryApiClient _mockCategoryClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="EditCategoryPageTests"/> class.
	/// </summary>
	public EditCategoryPageTests()
	{
		_ctx = new BunitContext();
		_mockCategoryClient = Substitute.For<ICategoryApiClient>();
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
		_mockCategoryClient.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
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
	public void EditCategoryPage_ShowsNotFoundMessage_WhenCategoryIsNull()
	{
		// Arrange — mock returns null by default

		// Act
		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, "nonexistent-id"));

		// Assert
		cut.Markup.Should().Contain("Category not found.");
	}

	[Fact]
	public void EditCategoryPage_ShowsEditHeading_WhenCategoryLoads()
	{
		// Arrange
		var category = MakeCategory("Feature");
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(category));

		// Act
		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, category.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Edit Category");
	}

	[Fact]
	public void EditCategoryPage_PrePopulatesNameInput_WhenCategoryLoads()
	{
		// Arrange
		var category = MakeCategory("Performance");
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(category));

		// Act
		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, category.Id.ToString()));

		// Assert
		var nameInput = cut.Find("#category-name");
		nameInput.GetAttribute("value").Should().Be("Performance");
	}

	[Fact]
	public void EditCategoryPage_HasSubmitButton_WithUpdateText()
	{
		// Arrange
		var category = MakeCategory("UI");
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(category));

		// Act
		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, category.Id.ToString()));

		// Assert
		cut.Find("button[type='submit']").TextContent.Should().Contain("Update Category");
	}

	[Fact]
	public async Task EditCategoryPage_SubmitForm_CallsUpdateAsync()
	{
		// Arrange
		var category = MakeCategory("Security");
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(category));

		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, category.Id.ToString()));

		// Act — change the name and submit
		await cut.Find("#category-name").ChangeAsync("Updated Security");
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockCategoryClient.Received(1)
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditCategoryPage_AfterSuccessfulUpdate_NavigatesToCategoriesList()
	{
		// Arrange
		var category = MakeCategory("Network");
		_mockCategoryClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(category));

		var cut = _ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, category.Id.ToString()));

		await cut.Find("#category-name").ChangeAsync("Updated Network");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = _ctx.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public void EditCategoryPage_CallsGetByIdAsync_WithCorrectId()
	{
		// Arrange
		const string testId = "cat-test-123";

		// Act
		_ctx.Render<EditCategoryPage>(p =>
			p.Add(c => c.Id, testId));

		// Assert
		_mockCategoryClient.Received(1)
			.GetByIdAsync(testId, Arg.Any<CancellationToken>());
	}
}
