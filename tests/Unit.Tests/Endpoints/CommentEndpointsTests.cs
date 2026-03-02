// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentEndpointsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Tests.Unit.Endpoints;

/// <summary>
/// Endpoint tests for Comment API routes.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommentEndpointsTests : IDisposable
{
	private readonly ApiWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public CommentEndpointsTests()
	{
		_factory = new ApiWebApplicationFactory();
		_client = _factory.CreateClient();
	}

	public void Dispose()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[Fact]
	public async Task ListComments_ReturnsOk()
	{
		// Arrange
		IReadOnlyList<CommentDto> items = [];
		_factory.CommentRepository
			.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Ok(items));

		// Act
		var response = await _client.GetAsync("/api/v1/comments");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetComment_WithValidId_ReturnsOk_WhenCommentExists()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var commentDto = new CommentDto(
			id,
			"Test Comment",
			"Description",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);
		_factory.CommentRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(commentDto));

		// Act
		var response = await _client.GetAsync($"/api/v1/comments/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetComment_WithValidId_ReturnsNotFound_WhenCommentDoesNotExist()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_factory.CommentRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Not found"));

		// Act
		var response = await _client.GetAsync($"/api/v1/comments/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetComment_WithInvalidId_ReturnsNotFound()
	{
		// Act
		var response = await _client.GetAsync("/api/v1/comments/not-a-valid-id");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task CreateComment_WithValidCommand_ReturnsCreated()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var commentDto = new CommentDto(
			commentId,
			"Test Comment",
			"Test comment text",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);
		_factory.CommentRepository
			.CreateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(commentDto));

		var command = new { Title = "Test Comment", CommentText = "Test comment text", IssueId = ObjectId.GenerateNewId().ToString() };

		// Act
		var response = await _client.PostAsJsonAsync("/api/v1/comments", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}

	[Fact]
	public async Task UpdateComment_WithValidCommand_ReturnsOk()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var commentDto = new CommentDto(
			commentId,
			"Updated Comment",
			"Updated comment text",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);
		_factory.CommentRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(commentDto));
		_factory.CommentRepository
			.UpdateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(commentDto));

		var command = new { Title = "Updated Comment", CommentText = "Updated comment text" };

		// Act
		var response = await _client.PatchAsJsonAsync($"/api/v1/comments/{commentId}", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task DeleteComment_WithValidId_ReturnsNoContent()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var commentDto = new CommentDto(
			commentId,
			"Test Comment",
			"Description",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);
		_factory.CommentRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(commentDto));
		_factory.CommentRepository
			.ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var response = await _client.DeleteAsync($"/api/v1/comments/{commentId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}
}
