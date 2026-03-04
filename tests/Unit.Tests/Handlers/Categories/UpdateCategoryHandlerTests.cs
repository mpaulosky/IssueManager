// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Categories;

/// <summary>
/// Unit tests for UpdateCategoryHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly UpdateCategoryValidator _validator;
	private readonly UpdateCategoryHandler _handler;

	public UpdateCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new UpdateCategoryValidator();
		_handler = new UpdateCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedCategory()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new CategoryDto(categoryId, "Old Name", "Old Description", DateTime.UtcNow, null, false, UserDto.Empty);
		var updatedCategory = new CategoryDto(categoryId, "Updated Name", "Updated Description", DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new UpdateCategoryCommand
		{
			Id = categoryId,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(existingCategory));

		_repository.UpdateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(updatedCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be("Updated Name");
		result.CategoryDescription.Should().Be("Updated Description");
		await _repository.Received(1).UpdateAsync(Arg.Is<CategoryDto>(c =>
			c.CategoryName == command.CategoryName &&
			c.CategoryDescription == command.CategoryDescription), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyCategoryName_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "",
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category name*required*");
	}

	[Fact]
	public async Task Handle_CategoryNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = new string('A', 101),
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category name*100 characters*");
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var command = new UpdateCategoryCommand
		{
			Id = categoryId,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{categoryId}*");
	}

	[Fact]
	public async Task Handle_RepositoryUpdateFails_ThrowsNotFoundException()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new CategoryDto(categoryId, "Old Name", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new UpdateCategoryCommand
		{
			Id = categoryId,
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(existingCategory));

		_repository.UpdateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Update failed"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage("*could not be updated*");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new CategoryDto(categoryId, "Old Name", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);
		var returnedCategory = new CategoryDto(categoryId, "Updated Name", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new UpdateCategoryCommand
		{
			Id = categoryId,
			CategoryName = "Updated Name",
			CategoryDescription = null
		};

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(existingCategory));

		_repository.UpdateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(returnedCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CategoryDescription.Should().BeEmpty();
	}
}
