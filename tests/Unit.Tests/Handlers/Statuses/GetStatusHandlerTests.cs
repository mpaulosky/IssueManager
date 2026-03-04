// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for GetStatusHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetStatusHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly GetStatusHandler _handler;

	public GetStatusHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_handler = new GetStatusHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidStatusId_ReturnsStatus()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(new StatusDto(
				statusId, "Open", "Issue is open", DateTime.UtcNow, null, false, UserDto.Empty)));

		var query = new GetStatusQuery(statusId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.StatusName.Should().Be("Open");
		result.StatusDescription.Should().Be("Issue is open");
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentStatusId_ReturnsNull()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		var query = new GetStatusQuery(statusId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		_repository.GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));
		var query = new GetStatusQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.Received(1).GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidStatusId_PassesCancellationToken()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(new StatusDto(
				statusId, "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)));

		var query = new GetStatusQuery(statusId);

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
	}
}
