// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteCategoryHandler (soft-delete via Archived) with a real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class DeleteCategoryHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICategoryRepository _repository = null!;
	private DeleteCategoryHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CategoryRepository(connectionString, TestDatabase);
		_handler = new DeleteCategoryHandler(_repository, new DeleteCategoryValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static CategoryDto CreateTestCategoryDto(string name, string description = "Test description", bool archived = false) =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, archived, UserDto.Empty);

	[Fact]
	public async Task Handle_ValidCategory_SetsArchivedInDatabase()
	{
		// Arrange - Create a category
		var category = CreateTestCategoryDto("Category to Delete", "This will be archived");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var command = new DeleteCategoryCommand { Id = created.Value!.Id };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeTrue();

		// Verify Archived is set in a database
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		getResult.Should().NotBeNull();
		getResult.Value!.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new DeleteCategoryCommand { Id = nonExistentId };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_AlreadyArchivedCategory_IsIdempotent()
	{
		// Arrange - Create an already archived category
		var archivedCategory = CreateTestCategoryDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedCategory, TestContext.Current.CancellationToken);

		var command = new DeleteCategoryCommand { Id = created.Value?.Id };

		// Act - Delete already archived category (should be idempotent)
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still return true
		result.Should().BeTrue();

		var dbCategoryResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		dbCategoryResult.Should().NotBeNull();
		dbCategoryResult.Value!.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_CategoryNotDeleted_RecordStillExists()
	{
		// Arrange - Create a category
		var category = CreateTestCategoryDto("Category to Archive", "Should still exist in DB");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var command = new DeleteCategoryCommand { Id = created.Value!.Id };

		// Act - Soft delete
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Record should still exist (soft delete)
		var dbCategory = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbCategory.Should().NotBeNull();
		dbCategory.Value!.Id.Should().Be(created.Value.Id);
		dbCategory.Value.Archived.Should().BeTrue();
	}
}
