// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCommentHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetCommentHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class GetCommentHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICommentRepository _repository = null!;
	private GetCommentHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CommentRepository(connectionString, TestDatabase);
		_handler = new GetCommentHandler(_repository);
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
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

		var query = new GetCommentQuery(created.Value.Id.ToString());

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.Title.Should().Be("Test Comment");
		result.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentComment_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var query = new GetCommentQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_InvalidObjectIdFormat_ReturnsNull()
	{
		// Arrange
		var query = new GetCommentQuery("invalid-id-format");

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetCommentQuery(string.Empty);

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Comment ID cannot be empty.*");
	}
}
