// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateCategoryHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class UpdateCategoryHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICategoryRepository _repository = null!;
	private UpdateCategoryHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CategoryRepository(connectionString, TestDatabase);
		_handler = new UpdateCategoryHandler(_repository, new UpdateCategoryValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
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
			Id = created.Value.Id.ToString(),
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be("Updated Name");
		result.CategoryDescription.Should().Be("Updated Description");
		result.Id.Should().Be(created.Value.Id);
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var command = new UpdateCategoryCommand
		{
			Id = nonExistentId,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"Category with ID '{nonExistentId}' was not found.");
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty name is invalid
		var category = CreateTestCategoryDto("Test Category", "Test Description");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var command = new UpdateCategoryCommand
		{
			Id = created.Value.Id.ToString(),
			CategoryName = string.Empty,
			CategoryDescription = "Updated Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
	}
}
