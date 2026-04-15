// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteCategoryHandler (soft-delete via Archived) with a real MongoDB database.
/// </summary>
[Collection("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class DeleteCategoryHandlerIntegrationTests
{
	private readonly ICategoryRepository _repository;
	private readonly DeleteCategoryHandler _handler;

	public DeleteCategoryHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CategoryRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new DeleteCategoryHandler(_repository);
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
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();

		// Verify Archived is set in the database
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		getResult.Should().NotBeNull();
		getResult.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ReturnsNotFoundFailure()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new DeleteCategoryCommand { Id = nonExistentId };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_AlreadyArchivedCategory_IsIdempotent()
	{
		// Arrange - Create an already archived category
		var archivedCategory = CreateTestCategoryDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedCategory, TestContext.Current.CancellationToken);

		var command = new DeleteCategoryCommand { Id = created.Value!.Id };

		// Act - Delete already archived category (should be idempotent)
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still return true
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();

		var dbCategoryResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbCategoryResult.Should().NotBeNull();
		dbCategoryResult.Value?.Archived.Should().BeTrue();
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
		dbCategory.Value?.Id.Should().Be(created.Value.Id);
		dbCategory.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_CreatedAndDeletedCategory_NotReturnedInList()
	{
		// Arrange - Create a category via repository
		var category = CreateTestCategoryDto("Category for List Test", "Will be archived");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);
		created.Value.Should().NotBeNull();

		var command = new DeleteCategoryCommand { Id = created.Value!.Id };

		// Act - Archive the category
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - GetAll (paginated) should exclude archived categories
		var result = await _repository.GetAllAsync(1, 100, TestContext.Current.CancellationToken);
		var allCategories = result.Value.Items;
		allCategories.Should().NotContain(c => c.Id == created.Value.Id);
	}
}
