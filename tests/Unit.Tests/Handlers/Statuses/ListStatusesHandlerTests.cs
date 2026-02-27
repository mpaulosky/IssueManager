// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListStatusesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Statuses;

using Shared.Abstractions;
using Shared.Models;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for ListStatusesHandler.
/// </summary>
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
		IEnumerable<Status> statuses = new List<Status>
		{
			new() { Id = ObjectId.GenerateNewId(), StatusName = "Open", StatusDescription = "Issue is open" },
			new() { Id = ObjectId.GenerateNewId(), StatusName = "Closed", StatusDescription = "Issue is closed" },
			new() { Id = ObjectId.GenerateNewId(), StatusName = "In Progress", StatusDescription = "Work in progress" }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Status>>.Ok(statuses));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

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
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Status>>.Ok((IEnumerable<Status>)new List<Status>()));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Status>>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Status>>.Ok((IEnumerable<Status>?)null!));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_PassesCancellationToken()
	{
		// Arrange
		var cancellationToken = new CancellationToken();
		IEnumerable<Status> statuses = new List<Status>
		{
			new() { Id = ObjectId.GenerateNewId(), StatusName = "Test" }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Status>>.Ok(statuses));

		// Act
		await _handler.Handle(cancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync();
	}
}
