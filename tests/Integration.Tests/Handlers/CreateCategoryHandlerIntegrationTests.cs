// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for CreateCategoryHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICategoryRepository _repository = null!;
	private CreateCategoryHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CategoryRepository(connectionString, TestDatabase);
		_handler = new CreateCategoryHandler(_repository, new CreateCategoryValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	[Fact]
	public async Task Handle_ValidCommand_CreatesCategory()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "New Category",
			CategoryDescription = "New Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be("New Category");
		result.CategoryDescription.Should().Be("New Description");
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Archived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty category name is invalid
		var command = new CreateCategoryCommand
		{
			CategoryName = string.Empty,
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
	}

	[Fact]
	public async Task Handle_CreatedCategory_CanBeRetrieved()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Retrievable Category",
			CategoryDescription = "Test Description"
		};

		// Act - Create category
		var created = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Verify it can be retrieved
		var retrieved = await _repository.GetByIdAsync(created.Id, TestContext.Current.CancellationToken);
		retrieved.Should().NotBeNull();
		retrieved.Value.CategoryName.Should().Be("Retrievable Category");
		retrieved.Value.CategoryDescription.Should().Be("Test Description");
	}
}
