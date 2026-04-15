// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Issues;

/// <summary>
/// Unit tests for UpdateIssueHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly ICategoryRepository _categoryRepository;
	private readonly IStatusRepository _statusRepository;
	private readonly UpdateIssueValidator _validator;
	private readonly UpdateIssueHandler _handler;

	public UpdateIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_categoryRepository = Substitute.For<ICategoryRepository>();
		_statusRepository = Substitute.For<IStatusRepository>();
		_validator = new UpdateIssueValidator();
		_handler = new UpdateIssueHandler(_repository, _categoryRepository, _statusRepository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Updated Title");
		result.Value!.Description.Should().Be("Updated Description");

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(Arg.Is<IssueDto>(i =>
				i.Title == command.Title &&
				i.Description == command.Description), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ReturnsValidationFailure()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "",
			Description = "Some description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_TitleTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = new string('A', 257),
			Description = "Some description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_DescriptionTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Valid Title",
			Description = new string('X', 4097)
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ReturnsNotFoundFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ReturnsConflictFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var archivedIssue = new IssueDto(
			issueId,
			"Archived Issue",
			"This is archived",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			true,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(archivedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Conflict);
	}

	[Fact]
	public async Task Handle_IdempotentUpdate_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Same Title",
			"Same Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Same Title",
			Description = "Same Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with { };

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Same Title");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = null
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Value!.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithCategoryId_UpdatesCategory()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var categoryId = ObjectId.GenerateNewId();
		var newCategory = new CategoryDto(categoryId, "New Category", "New cat desc", DateTime.UtcNow, null, false, UserDto.Empty);

		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			CategoryId = categoryId
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(newCategory));

		var updatedIssue = existingIssue with { Title = command.Title, Description = string.Empty, Category = newCategory };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Category.CategoryName.Should().Be("New Category");
		await _categoryRepository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithStatusId_UpdatesStatus()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var statusId = ObjectId.GenerateNewId();
		var newStatus = new StatusDto(statusId, "In Progress", "Being worked on", DateTime.UtcNow, null, false, UserDto.Empty);

		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			StatusId = statusId
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_statusRepository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(newStatus));

		var updatedIssue = existingIssue with { Title = command.Title, Description = string.Empty, Status = newStatus };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Status.StatusName.Should().Be("In Progress");
		await _statusRepository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WithInvalidCategoryId_ReturnsNotFoundFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var categoryId = ObjectId.GenerateNewId();

		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			CategoryId = categoryId
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<CategoryDto>("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_WithInvalidStatusId_ReturnsNotFoundFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var statusId = ObjectId.GenerateNewId();

		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			StatusId = statusId
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_statusRepository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<StatusDto>("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}
}
