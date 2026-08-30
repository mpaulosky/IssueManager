// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for the generic create path (TaxonomyCrudHandler via CategoryTaxonomyAdapter), with a real MongoDB database.
/// </summary>
[Collection("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerIntegrationTests
{
	private readonly ICategoryRepository _repository;
	private readonly TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> _handler;

	public CreateCategoryHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CategoryRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand>(_repository, CategoryTaxonomyAdapter.Instance);
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
		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be("New Category");
		result.Value!.CategoryDescription.Should().Be("New Description");
		result.Value!.Id.Should().NotBe(ObjectId.Empty);
		result.Value!.Archived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ReturnsValidationFailure()
	{
		// Arrange - Empty category name is invalid
		var command = new CreateCategoryCommand
		{
			CategoryName = string.Empty,
			CategoryDescription = "Description"
		};

		// Act
		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
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
		var created = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert - Verify it can be retrieved
		var retrieved = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		retrieved.Should().NotBeNull();
		retrieved.Value?.CategoryName.Should().Be("Retrievable Category");
		retrieved.Value?.CategoryDescription.Should().Be("Test Description");
	}
}
