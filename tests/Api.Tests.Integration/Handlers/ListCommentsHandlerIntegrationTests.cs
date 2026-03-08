// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCommentsHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for ListCommentsHandler with a real MongoDB database.
/// </summary>
[Collection("CommentIntegration")]
[ExcludeFromCodeCoverage]
public class ListCommentsHandlerIntegrationTests
{
	private readonly ICommentRepository _repository;
	private readonly ListCommentsHandler _handler;

	public ListCommentsHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CommentRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new ListCommentsHandler(_repository);
	}

	private static CommentDto CreateTestCommentDto(string title, string description = "Test description", ObjectId? issueId = null) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null,
			issueId.HasValue ? new IssueDto(issueId.Value, "Issue", "Desc", DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false) : IssueDto.Empty,
			UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty);

	[Fact]
	public async Task Handle_EmptyDatabase_ReturnsEmptyList()
	{
		// Arrange - No comments in database

		// Act
		var result = await _handler.Handle(null, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithComments_ReturnsAll()
	{
		// Arrange - Create 3 comments
		var comment1 = CreateTestCommentDto("Comment 1", "Description 1");
		var comment2 = CreateTestCommentDto("Comment 2", "Description 2");
		var comment3 = CreateTestCommentDto("Comment 3", "Description 3");

		await _repository.CreateAsync(comment1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(comment2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(comment3, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.Handle(null, TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CommentDto> commentDtos = result.ToList();
		commentDtos.Should().HaveCount(3);
		commentDtos.Should().Contain(c => c.Title == "Comment 1");
		commentDtos.Should().Contain(c => c.Title == "Comment 2");
		commentDtos.Should().Contain(c => c.Title == "Comment 3");
	}

	[Fact]
	public async Task Handle_WithIssueIdFilter_ReturnsFilteredComments()
	{
		// Arrange - Create comments with different issue IDs
		var issueId1 = ObjectId.GenerateNewId();
		var issueId2 = ObjectId.GenerateNewId();

		var comment1 = CreateTestCommentDto("Comment 1", "Description 1", issueId1);
		var comment2 = CreateTestCommentDto("Comment 2", "Description 2", issueId1);
		var comment3 = CreateTestCommentDto("Comment 3", "Description 3", issueId2);

		await _repository.CreateAsync(comment1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(comment2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(comment3, TestContext.Current.CancellationToken);

		// Act - Filter by issueId1
		var result = await _handler.Handle(issueId1.ToString(), TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CommentDto> commentDtos = result.ToList();
		commentDtos.Should().HaveCount(2);
		commentDtos.Should().Contain(c => c.Title == "Comment 1");
		commentDtos.Should().Contain(c => c.Title == "Comment 2");
		commentDtos.Should().NotContain(c => c.Title == "Comment 3");
	}

	[Fact]
	public async Task Handle_NoMatchingIssueId_ReturnsEmptyList()
	{
		// Arrange - Create comments
		var comment1 = CreateTestCommentDto("Comment 1", "Description 1", ObjectId.GenerateNewId());
		await _repository.CreateAsync(comment1, TestContext.Current.CancellationToken);

		// Act - Filter by non-existent issue ID
		var nonExistentIssueId = ObjectId.GenerateNewId().ToString();
		var result = await _handler.Handle(nonExistentIssueId, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}
}
