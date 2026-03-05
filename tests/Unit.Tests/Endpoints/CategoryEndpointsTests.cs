// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryEndpointsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Endpoints;

/// <summary>
/// Endpoint tests for Category API routes.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryEndpointsTests : IDisposable
{
	private readonly ApiWebApplicationFactory _factory;
	private readonly HttpClient _client;
	private readonly HttpClient _authenticatedClient;

	public CategoryEndpointsTests()
	{
		_factory = new ApiWebApplicationFactory();
		_client = _factory.CreateClient();
		_authenticatedClient = _factory.CreateAuthenticatedClient();
	}

	public void Dispose()
	{
		_authenticatedClient.Dispose();
		_client.Dispose();
		_factory.Dispose();
	}

	[Fact]
	public async Task ListCategories_ReturnsOk()
	{
		// Arrange
		IReadOnlyList<CategoryDto> items = [];
		_factory.CategoryRepository
			.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Ok(items));

		// Act
		var response = await _client.GetAsync("/api/v1/categories").ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetCategory_WithValidId_ReturnsOk_WhenCategoryExists()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var categoryDto = new CategoryDto(
			id,
			"Test Category",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.CategoryRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(categoryDto));

		// Act
		var response = await _client.GetAsync($"/api/v1/categories/{id}").ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetCategory_WithValidId_ReturnsNotFound_WhenCategoryDoesNotExist()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_factory.CategoryRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));

		// Act
		var response = await _client.GetAsync($"/api/v1/categories/{id}").ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetCategory_WithInvalidId_ReturnsNotFound()
	{
		// Act
		var response = await _client.GetAsync("/api/v1/categories/not-a-valid-id").ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateCategory_WithValidCommand_ReturnsCreated()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categoryDto = new CategoryDto(
			categoryId,
			"Test Category",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.CategoryRepository
			.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(categoryDto));

		var command = new { CategoryName = "Test Category", CategoryDescription = "Description" };

		// Act
		var response = await _authenticatedClient.PostAsJsonAsync("/api/v1/categories", command).ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}

	[Fact]
	public async Task CreateCategory_WithoutAuthentication_ReturnsUnauthorized()
	{
		// Arrange
		var command = new { CategoryName = "Test Category", CategoryDescription = "Description" };

		// Act
		var response = await _client.PostAsJsonAsync("/api/v1/categories", command).ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task UpdateCategory_WithValidCommand_ReturnsOk()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categoryDto = new CategoryDto(
			categoryId,
			"Updated Category",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.CategoryRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(categoryDto));
		_factory.CategoryRepository
			.UpdateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(categoryDto));

		var command = new { CategoryName = "Updated Category", CategoryDescription = "Description" };

		// Act
		var response = await _authenticatedClient.PatchAsJsonAsync($"/api/v1/categories/{categoryId}", command).ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task UpdateCategory_WithoutAuthentication_ReturnsUnauthorized()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var command = new { CategoryName = "Updated Category", CategoryDescription = "Description" };

		// Act
		var response = await _client.PatchAsJsonAsync($"/api/v1/categories/{categoryId}", command).ConfigureAwait(false);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

}
