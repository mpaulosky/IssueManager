// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Categories;

/// <summary>
/// Unit tests for DeleteCategoryHandler (soft-delete via Archived).
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly DeleteCategoryValidator _validator;
	private readonly DeleteCategoryHandler _handler;

	public DeleteCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new DeleteCategoryValidator();
		_handler = new DeleteCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCategory_SetsIsArchivedToTrue()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var category = new CategoryDto(categoryId, "Bug", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new DeleteCategoryCommand { Id = categoryId };

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		_repository.ArchiveAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(categoryId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var command = new DeleteCategoryCommand { Id = categoryId };

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{categoryId}*");
	}

	[Fact]
	public async Task Handle_AlreadyArchivedCategory_IsIdempotent()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var archivedCategory = new CategoryDto(categoryId, "Archived", string.Empty, DateTime.UtcNow, null, true, UserDto.Empty);

		var command = new DeleteCategoryCommand { Id = categoryId };

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(archivedCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_InvalidId_ThrowsValidationException()
	{
		// Arrange
		var command = new DeleteCategoryCommand { Id = ObjectId.Empty };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category ID*required*");
	}

	[Fact]
	public async Task Handle_ValidCategory_PassesCancellationToken()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var category = new CategoryDto(categoryId, "Test", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new DeleteCategoryCommand { Id = categoryId };

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		_repository.ArchiveAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(categoryId, Arg.Any<CancellationToken>());
	}
}
