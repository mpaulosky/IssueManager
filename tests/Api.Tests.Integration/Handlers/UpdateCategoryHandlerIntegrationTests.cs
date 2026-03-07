// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateCategoryHandler with a real MongoDB database.
/// </summary>
[Collection("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class UpdateCategoryHandlerIntegrationTests
{
	private readonly ICategoryRepository _repository;
	private readonly UpdateCategoryHandler _handler;

	public UpdateCategoryHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_repository = new CategoryRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new UpdateCategoryHandler(_repository, new UpdateCategoryValidator());
	}

	private static CategoryDto CreateTestCategoryDto(string name, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingCategory_UpdatesSuccessfully()
	{
		// Arrange - Create a category
		var category = CreateTestCategoryDto("Original Name", "Original Description");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var command = new UpdateCategoryCommand
		{
			Id = created.Value!.Id,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be("Updated Name");
		result.Value!.CategoryDescription.Should().Be("Updated Description");
		result.Value!.Id.Should().Be(created.Value.Id);
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new UpdateCategoryCommand
		{
			Id = nonExistentId,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty name is invalid
		var category = CreateTestCategoryDto("Test Category", "Test Description");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var command = new UpdateCategoryCommand
		{
			Id = created.Value!.Id,
			CategoryName = string.Empty,
			CategoryDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
