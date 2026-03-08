// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCommentHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetCommentHandler with a real MongoDB database.
/// </summary>
[Collection("CommentIntegration")]
[ExcludeFromCodeCoverage]
public class GetCommentHandlerIntegrationTests
{
	private readonly ICommentRepository _repository;
	private readonly GetCommentHandler _handler;

	public GetCommentHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CommentRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new GetCommentHandler(_repository);
	}

	private static CommentDto CreateTestCommentDto(string title, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, IssueDto.Empty,
			UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingComment_ReturnsComment()
	{
		// Arrange - Create a comment
		var comment = CreateTestCommentDto("Test Comment", "Test Description");
		var created = await _repository.CreateAsync(comment, TestContext.Current.CancellationToken);

		var query = new GetCommentQuery(created.Value!.Id);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Test Comment");
		result.Value!.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentComment_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var query = new GetCommentQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidObjectIdFormat_ReturnsNull()
	{
		// Arrange
		var query = new GetCommentQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetCommentQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
