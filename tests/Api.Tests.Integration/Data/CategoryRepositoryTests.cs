// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryRepositoryTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Data;

/// <summary>
/// Integration tests for CategoryRepository using a real MongoDB container.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryRepositoryTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
			.Build();

	private ICategoryRepository _repository = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CategoryRepository(connectionString, TestDatabase);
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static CategoryDto CreateTestCategory(string name = "Test Category", string description = "Test Description") =>
		new(
			ObjectId.GenerateNewId(),
			name,
			description,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

	[Fact]
	public async Task CreateAsync_WithValidCategory_ReturnsOkResult()
	{
		// Arrange
		var category = CreateTestCategory();

		// Act
		var result = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_WithValidCategory_ReturnsCreatedCategory()
	{
		// Arrange
		var category = CreateTestCategory("New Category", "New Description");

		// Act
		var result = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value?.CategoryName.Should().Be("New Category");
	}

	[Fact]
	public async Task GetByIdAsync_WithExistingId_ReturnsCategory()
	{
		// Arrange
		var category = CreateTestCategory();
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value?.CategoryName.Should().Be(category.CategoryName);
	}

	[Fact]
	public async Task GetByIdAsync_WithNonExistentId_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task GetAllAsync_WithCategories_ReturnsAllCategories()
	{
		// Arrange
		await _repository.CreateAsync(CreateTestCategory("Category 1", "Description 1"), TestContext.Current.CancellationToken);
		await _repository.CreateAsync(CreateTestCategory("Category 2", "Description 2"), TestContext.Current.CancellationToken);
		await _repository.CreateAsync(CreateTestCategory("Category 3", "Description 3"), TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
	{
		// Act
		var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task ArchiveAsync_WithExistingId_ReturnsOkResult()
	{
		// Arrange
		var category = CreateTestCategory();
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.ArchiveAsync(created.Value!.Id, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_WithExistingId_SetsArchivedTrue()
	{
		// Arrange
		var category = CreateTestCategory();
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		// Act
		await _repository.ArchiveAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert
		getResult.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_WithNonExistentId_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.ArchiveAsync(nonExistentId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task UpdateAsync_WithExistingCategory_ReturnsOkResult()
	{
		// Arrange
		var category = CreateTestCategory();
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);
		var updated = created.Value! with { CategoryName = "Updated Name" };

		// Act
		var result = await _repository.UpdateAsync(updated, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task UpdateAsync_WithExistingCategory_PersistsChanges()
	{
		// Arrange
		var category = CreateTestCategory();
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);
		var updated = created.Value! with { CategoryName = "Updated Name" };

		// Act
		await _repository.UpdateAsync(updated, TestContext.Current.CancellationToken);
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert
		getResult.Value?.CategoryName.Should().Be("Updated Name");
	}

	[Fact]
	public async Task UpdateAsync_WithNonExistentCategory_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentCategory = CreateTestCategory() with { Id = ObjectId.GenerateNewId() };

		// Act
		var result = await _repository.UpdateAsync(nonExistentCategory, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}
}
