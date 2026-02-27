// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCommentsHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using Api.Data;
using Api.Handlers;
using Api.Handlers.Comments;

using MongoDB.Bson;
using Shared.Abstractions;
using Shared.Models;

namespace Tests.Unit.Handlers.Comments;

/// <summary>
/// Unit tests for ListCommentsHandler.
/// </summary>
public class ListCommentsHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly ListCommentsHandler _handler;

	public ListCommentsHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_handler = new ListCommentsHandler(_repository);
	}

	[Fact]
	public async Task Handle_ReturnsAllComments()
	{
		// Arrange
		IEnumerable<Comment> comments = new List<Comment>
		{
			new() { Id = ObjectId.GenerateNewId(), Title = "First Comment", Description = "First comment text." },
			new() { Id = ObjectId.GenerateNewId(), Title = "Second Comment", Description = "Second comment text." },
			new() { Id = ObjectId.GenerateNewId(), Title = "Third Comment", Description = "Third comment text." }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Comment>>.Ok(comments));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(c => c.Title == "First Comment");
		result.Should().Contain(c => c.Title == "Second Comment");
		result.Should().Contain(c => c.Title == "Third Comment");
	}

	[Fact]
	public async Task Handle_NoComments_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Comment>>.Ok((IEnumerable<Comment>)new List<Comment>()));

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
			.Returns(Result<IEnumerable<Comment>>.Fail("Database error"));

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
			.Returns(Result<IEnumerable<Comment>>.Ok((IEnumerable<Comment>?)null!));

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
		IEnumerable<Comment> comments = new List<Comment>
		{
			new() { Id = ObjectId.GenerateNewId(), Title = "Test Comment" }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Comment>>.Ok(comments));

		// Act
		await _handler.Handle(cancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync();
	}
}
