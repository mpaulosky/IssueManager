// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetIssueHandler with a real MongoDB database.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class GetIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly GetIssueHandler _handler;

	public GetIssueHandlerTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new GetIssueHandler(_repository);
	}

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);

	[Fact]
	public async Task Handle_ExistingIssueId_ReturnsIssue()
	{
		// Arrange
		var issue = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		var query = new GetIssueQuery(created.Value!.Id);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Id.Should().Be(created.Value.Id);
		result.Value!.Title.Should().Be("Test Issue");
		result.Value!.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistingIssueId_ReturnsNull()
	{
		// Arrange
		var query = new GetIssueQuery(ObjectId.GenerateNewId());

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		var query = new GetIssueQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task HandleGetAll_MultipleIssues_ReturnsAllIssues()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("Issue 1", "Description 1");
		var issue2 = CreateTestIssueDto("Issue 2", "Description 2");
		var issue3 = CreateTestIssueDto("Issue 3", "Description 3");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleGetAll(TestContext.Current.CancellationToken);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(i => i.Title == "Issue 1");
		result.Should().Contain(i => i.Title == "Issue 2");
		result.Should().Contain(i => i.Title == "Issue 3");
	}

	[Fact]
	public async Task HandleGetAll_EmptyDatabase_ReturnsEmptyList()
	{
		// Act
		var result = await _handler.HandleGetAll(TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}
