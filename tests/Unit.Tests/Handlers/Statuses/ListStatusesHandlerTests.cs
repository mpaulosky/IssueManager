// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListStatusesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for ListStatusesHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class ListStatusesHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly ListStatusesHandler _handler;

	public ListStatusesHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_handler = new ListStatusesHandler(_repository);
	}

	[Fact]
	public async Task Handle_ReturnsAllStatuses()
	{
		// Arrange
		IReadOnlyList<StatusDto> statuses = new List<StatusDto>
		{
			new(ObjectId.GenerateNewId(), "Open", "Issue is open", DateTime.UtcNow, null, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "In Progress", "Work in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Ok(statuses));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(s => s.StatusName == "Open");
		result.Should().Contain(s => s.StatusName == "Closed");
		result.Should().Contain(s => s.StatusName == "In Progress");
	}

	[Fact]
	public async Task Handle_NoStatuses_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Ok((IReadOnlyList<StatusDto>)new List<StatusDto>()));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Ok((IReadOnlyList<StatusDto>)null!));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_PassesCancellationToken()
	{
		// Arrange
		IReadOnlyList<StatusDto> statuses = new List<StatusDto>
		{
			new(ObjectId.GenerateNewId(), "Test", string.Empty, DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Ok(statuses));

		// Act
		await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}
}
