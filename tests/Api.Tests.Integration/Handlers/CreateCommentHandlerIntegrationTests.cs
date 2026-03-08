// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for CreateCommentHandler with a real MongoDB database.
/// </summary>
[Collection("CommentIntegration")]
[ExcludeFromCodeCoverage]
public class CreateCommentHandlerIntegrationTests
{
	private readonly ICommentRepository _repository;
	private readonly CreateCommentHandler _handler;

	public CreateCommentHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CommentRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");

		var currentUserService = Substitute.For<ICurrentUserService>();
		currentUserService.IsAuthenticated.Returns(false);

		_handler = new CreateCommentHandler(_repository, new CreateCommentValidator(), currentUserService);
	}

	[Fact]
	public async Task Handle_ValidCommand_CreatesComment()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "New Comment",
			CommentText = "New comment text",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("New Comment");
		result.Description.Should().Be("New comment text");
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Archived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty title is invalid
		var command = new CreateCommentCommand
		{
			Title = string.Empty,
			CommentText = "Some text"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
	}

	[Fact]
	public async Task Handle_CreatedComment_CanBeRetrieved()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Retrievable Comment",
			CommentText = "Test comment text",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act - Create comment
		var created = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Verify it can be retrieved
		var retrieved = await _repository.GetByIdAsync(created.Id, TestContext.Current.CancellationToken);
		retrieved.Should().NotBeNull();
		retrieved.Value?.Title.Should().Be("Retrievable Comment");
		retrieved.Value?.Description.Should().Be("Test comment text");
	}
}
