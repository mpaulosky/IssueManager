// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Shared.Abstractions;
using Shared.Models;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for GetStatusHandler.
/// </summary>
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
		var status = new Status
		{
			Id = statusId,
			StatusName = "Open",
			StatusDescription = "Issue is open",
			DateCreated = DateTime.UtcNow
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Ok(status));

		var query = new GetStatusQuery(statusId.ToString());

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.StatusName.Should().Be("Open");
		result.StatusDescription.Should().Be("Issue is open");
		await _repository.Received(1).GetAsync(statusId);
	}

	[Fact]
	public async Task Handle_NonExistentStatusId_ReturnsNull()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Fail("Not found"));

		var query = new GetStatusQuery(statusId.ToString());

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyStatusId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetStatusQuery("");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Status ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_WhitespaceStatusId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetStatusQuery("   ");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Status ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ReturnsNull()
	{
		// Arrange
		var query = new GetStatusQuery("invalid-object-id");

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.DidNotReceive().GetAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task Handle_ValidStatusId_PassesCancellationToken()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var status = new Status
		{
			Id = statusId,
			StatusName = "Closed",
			StatusDescription = "Issue is closed"
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Ok(status));

		var query = new GetStatusQuery(statusId.ToString());

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetAsync(statusId);
	}
}
