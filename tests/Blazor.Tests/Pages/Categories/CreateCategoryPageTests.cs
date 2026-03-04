// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

namespace BlazorTests.Pages.Categories;

/// <summary>
/// bUnit tests for the <see cref="CreateCategoryPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCategoryPageTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly ICategoryApiClient _mockCategoryClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCategoryPageTests"/> class.
	/// </summary>
	public CreateCategoryPageTests()
	{
		_ctx = new BunitContext();
		_mockCategoryClient = Substitute.For<ICategoryApiClient>();
		_mockCategoryClient
			.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<CategoryDto?>(null));
		_ctx.Services.AddSingleton(_mockCategoryClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	[Fact]
	public void CreateCategoryPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<CreateCategoryPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Create Category");
	}

	[Fact]
	public void CreateCategoryPage_HasNameInput()
	{
		// Act
		var cut = _ctx.Render<CreateCategoryPage>();

		// Assert
		cut.Find("#category-name").Should().NotBeNull();
	}

	[Fact]
	public void CreateCategoryPage_HasDescriptionInput()
	{
		// Act
		var cut = _ctx.Render<CreateCategoryPage>();

		// Assert
		cut.Find("#category-description").Should().NotBeNull();
	}

	[Fact]
	public void CreateCategoryPage_HasSubmitButton()
	{
		// Act
		var cut = _ctx.Render<CreateCategoryPage>();

		// Assert
		var submitButton = cut.Find("button[type='submit']");
		submitButton.Should().NotBeNull();
		submitButton.TextContent.Should().Contain("Create Category");
	}

	[Fact]
	public void CreateCategoryPage_HasCancelLink()
	{
		// Act
		var cut = _ctx.Render<CreateCategoryPage>();

		// Assert
		var cancelLink = cut.Find("a[href='/categories']");
		cancelLink.Should().NotBeNull();
	}

	[Fact]
	public async Task CreateCategoryPage_SubmitWithValidName_CallsCreateAsync()
	{
		// Arrange
		var cut = _ctx.Render<CreateCategoryPage>();
		await cut.Find("#category-name").ChangeAsync("New Category Name");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockCategoryClient.Received(1)
			.CreateAsync(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateCategoryPage_AfterSuccessfulSubmit_NavigatesToCategoriesList()
	{
		// Arrange
		var cut = _ctx.Render<CreateCategoryPage>();
		await cut.Find("#category-name").ChangeAsync("Valid Category");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = _ctx.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/categories");
	}
}
