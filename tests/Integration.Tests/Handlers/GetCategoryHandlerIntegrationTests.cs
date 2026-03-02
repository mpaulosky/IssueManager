// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetCategoryHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class GetCategoryHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICategoryRepository _repository = null!;
	private GetCategoryHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CategoryRepository(connectionString, TestDatabase);
		_handler = new GetCategoryHandler(_repository);
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
	public async Task Handle_ExistingCategory_ReturnsCategory()
	{
		// Arrange - Create a category
		var category = CreateTestCategoryDto("Test Category", "Test Description");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var query = new GetCategoryQuery(created.Value.Id.ToString());

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result!.CategoryName.Should().Be("Test Category");
		result.CategoryDescription.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var query = new GetCategoryQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_InvalidObjectIdFormat_ReturnsNull()
	{
		// Arrange
		var query = new GetCategoryQuery("invalid-id-format");

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetCategoryQuery(string.Empty);

		// Act
		Func<Task> act = async () => await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Category ID cannot be empty.*");
	}
}
