// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers;

/// <summary>
/// Unit tests for the generic DeleteHandler{TDto} (soft-delete via Archived). Exercised once
/// through CategoryDto/ICategoryRepository - the logic is identical for every entity, so this
/// single instantiation covers the shared behavior for all of them.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly DeleteHandler<CategoryDto> _handler;

	public DeleteHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_handler = new DeleteHandler<CategoryDto>(_repository, "Category");
	}

	[Fact]
	public async Task Handle_ValidId_SetsArchivedToTrue()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var category = new CategoryDto(
			categoryId,
			"Test Category",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		_repository.ArchiveAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(categoryId, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(categoryId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentId_ReturnsNotFoundResult()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));

		// Act
		var result = await _handler.Handle(categoryId, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_AlreadyArchived_IsIdempotent()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var archivedCategory = new CategoryDto(
			categoryId,
			"Archived Category",
			"Already archived",
			DateTime.UtcNow,
			null,
			true,
			UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(archivedCategory));

		// Act
		var result = await _handler.Handle(categoryId, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyId_ReturnsValidationFailure()
	{
		// Act
		var result = await _handler.Handle(ObjectId.Empty, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_RepositoryArchiveFails_ReturnsFailure()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var category = new CategoryDto(
			categoryId,
			"Test Category",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		_repository.ArchiveAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail("Archive failed"));

		// Act
		var result = await _handler.Handle(categoryId, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Archive failed");
	}

	[Fact]
	public async Task Handle_ValidId_PassesCancellationToken()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var category = new CategoryDto(
			categoryId,
			"Test Category",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		_repository.ArchiveAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(categoryId, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(categoryId, Arg.Any<CancellationToken>());
	}
}
