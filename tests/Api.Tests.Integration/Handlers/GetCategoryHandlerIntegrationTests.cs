// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetCategoryHandler with a real MongoDB database.
/// </summary>
[Collection("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class GetCategoryHandlerIntegrationTests
{
	private readonly ICategoryRepository _repository;
	private readonly GetCategoryHandler _handler;

	public GetCategoryHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CategoryRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new GetCategoryHandler(_repository);
	}

	private static CategoryDto CreateTestCategoryDto(string name, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingCategory_ReturnsCategory()
	{
		// Arrange - Create a category
		var category = CreateTestCategoryDto("Test Category", "Test Description");
		var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);

		var query = new GetCategoryQuery(created.Value!.Id);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be("Test Category");
		result.Value!.CategoryDescription.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var query = new GetCategoryQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		var query = new GetCategoryQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
