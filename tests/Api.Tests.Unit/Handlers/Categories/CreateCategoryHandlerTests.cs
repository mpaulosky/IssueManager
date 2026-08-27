// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Categories;

/// <summary>
/// Unit tests for CreateCategoryHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly CreateCategoryValidator _validator;
	private readonly CreateCategoryHandler _handler;

	public CreateCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new CreateCategoryValidator();
		_handler = new CreateCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedCategory()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = "Bug reports"
		};

		var createdCategory = new CategoryDto(ObjectId.GenerateNewId(), command.CategoryName, command.CategoryDescription!, DateTime.UtcNow, null, false, UserDto.Empty);

		_repository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(createdCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be(command.CategoryName);
		result.Value!.CategoryDescription.Should().Be(command.CategoryDescription);
		await _repository.Received(1).CreateAsync(Arg.Is<CategoryDto>(c =>
			c.CategoryName == command.CategoryName &&
			c.CategoryDescription == command.CategoryDescription), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyCategoryName_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "",
			CategoryDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Category name").And.Contain("required");
	}

	[Fact]
	public async Task Handle_CategoryNameTooShort_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "A",
			CategoryDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Category name").And.Contain("at least 2 characters");
	}

	[Fact]
	public async Task Handle_CategoryNameTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = new string('A', 101),
			CategoryDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Category name").And.Contain("100 characters");
	}

	[Fact]
	public async Task Handle_CategoryDescriptionTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = new string('X', 501)
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Category description").And.Contain("500 characters");
	}

	[Fact]
	public async Task Handle_NullCategoryDescription_UsesEmptyString()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = null
		};

		var returnedCategory = new CategoryDto(ObjectId.GenerateNewId(), command.CategoryName, string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);

		_repository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(returnedCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Value!.CategoryDescription.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsFailureResult()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = "Description"
		};

		_repository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}
}
