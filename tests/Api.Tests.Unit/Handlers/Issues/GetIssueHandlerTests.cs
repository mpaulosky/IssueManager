// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Builders;
using Api.Data.Interfaces;

namespace Api.Handlers.Issues;

/// <summary>
/// Unit tests for GetIssueHandler.
/// </summary>
[ExcludeFromCodeCoverage]
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
		var issueId = ObjectId.GenerateNewId();
		var expectedIssue = IssueBuilder.Default()
			.WithId(issueId.ToString())
			.WithTitle("Test Issue")
			.Build();

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(expectedIssue));

		var query = new GetIssueQuery(issueId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Id.ToString().Should().Be(issueId.ToString());
		result.Value!.Title.Should().Be("Test Issue");
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentIssueId_ReturnsFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));

		var query = new GetIssueQuery(issueId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Not found");
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsFailure()
	{
		// Arrange
		_repository.GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));
		var query = new GetIssueQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_ValidIssueId_PassesCancellationToken()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var expectedIssue = IssueBuilder.Default().WithId(issueId.ToString()).Build();

		_repository.GetByIdAsync(issueId, cancellationToken)
			.Returns(Result.Ok(expectedIssue));

		var query = new GetIssueQuery(issueId);

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(issueId, cancellationToken);
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
