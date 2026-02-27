// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Issues;

using Shared.Abstractions;
using Shared.DTOs;
using MongoDB.Bson;
using NSubstitute;
using Tests.Unit.Builders;

namespace Tests.Unit.Handlers.Issues;

/// <summary>
/// Unit tests for GetIssueHandler.
/// </summary>
public class GetIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly GetIssueHandler _handler;

	public GetIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_handler = new GetIssueHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidIssueId_ReturnsIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var expectedIssue = IssueBuilder.Default()
			.WithId(issueId)
			.WithTitle("Test Issue")
			.Build();

		_repository.GetByIdAsync(ObjectId.Parse(issueId), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(expectedIssue));

		var query = new GetIssueQuery(issueId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.Id.ToString().Should().Be(issueId);
		result.Title.Should().Be("Test Issue");
		await _repository.Received(1).GetByIdAsync(ObjectId.Parse(issueId), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentIssueId_ReturnsNull()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		_repository.GetByIdAsync(ObjectId.Parse(issueId), Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));

		var query = new GetIssueQuery(issueId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.Received(1).GetByIdAsync(ObjectId.Parse(issueId), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetIssueQuery("");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Issue ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_WhitespaceIssueId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetIssueQuery("   ");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Issue ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_ValidIssueId_PassesCancellationToken()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var cancellationToken = new CancellationToken();
		var expectedIssue = IssueBuilder.Default().WithId(issueId).Build();

		_repository.GetByIdAsync(ObjectId.Parse(issueId), cancellationToken)
			.Returns(Result.Ok(expectedIssue));

		var query = new GetIssueQuery(issueId);

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(ObjectId.Parse(issueId), cancellationToken);
	}

	[Fact]
	public async Task HandleGetAll_ReturnsAllIssues()
	{
		// Arrange
		var issues = new List<IssueDto>
		{
			IssueBuilder.Default().WithTitle("Issue 1").Build(),
			IssueBuilder.Default().WithTitle("Issue 2").Build(),
			IssueBuilder.Default().WithTitle("Issue 3").Build()
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok<IReadOnlyList<IssueDto>>(issues.AsReadOnly()));

		// Act
		var result = await _handler.HandleGetAll(CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(i => i.Title == "Issue 1");
		result.Should().Contain(i => i.Title == "Issue 2");
		result.Should().Contain(i => i.Title == "Issue 3");
	}

	[Fact]
	public async Task HandleGetAll_NoIssues_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok<IReadOnlyList<IssueDto>>(new List<IssueDto>().AsReadOnly()));

		// Act
		var result = await _handler.HandleGetAll(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
