// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueEndpointsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Tests.Unit.Endpoints;

/// <summary>
/// Endpoint tests for Issue API routes.
/// </summary>
[ExcludeFromCodeCoverage]
public class IssueEndpointsTests : IDisposable
{
	private readonly ApiWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public IssueEndpointsTests()
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
	public async Task ListIssues_ReturnsOk()
	{
		// Arrange
		IReadOnlyList<IssueDto> items = [];
		_factory.IssueRepository
			.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<(IReadOnlyList<IssueDto> Items, long Total)>.Ok((items, 0L)));

		// Act
		var response = await _client.GetAsync("/api/v1/issues");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetIssue_WithValidId_ReturnsOk_WhenIssueExists()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var issueDto = new IssueDto(
			id,
			"Test Issue",
			"Description",
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);
		_factory.IssueRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Ok(issueDto));

		// Act
		var response = await _client.GetAsync($"/api/v1/issues/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetIssue_WithValidId_ReturnsNotFound_WhenIssueDoesNotExist()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_factory.IssueRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Fail("Not found"));

		// Act
		var response = await _client.GetAsync($"/api/v1/issues/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetIssue_WithInvalidId_ReturnsNotFound()
	{
		// Act
		var response = await _client.GetAsync("/api/v1/issues/not-a-valid-id");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task CreateIssue_WithValidCommand_ReturnsCreated()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var issueDto = new IssueDto(
			issueId,
			"Test Issue",
			"Description",
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);
		_factory.IssueRepository
			.CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Ok(issueDto));

		var command = new { Title = "Test Issue", Description = "Description" };

		// Act
		var response = await _client.PostAsJsonAsync("/api/v1/issues", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}

	[Fact]
	public async Task UpdateIssue_WithValidCommand_ReturnsOk()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var issueDto = new IssueDto(
			issueId,
			"Updated Issue",
			"Description",
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);
		_factory.IssueRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Ok(issueDto));
		_factory.IssueRepository
			.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Ok(issueDto));

		var command = new { Title = "Updated Issue", Description = "Description" };

		// Act
		var response = await _client.PatchAsJsonAsync($"/api/v1/issues/{issueId}", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task DeleteIssue_WithValidId_ReturnsNoContent()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var issueDto = new IssueDto(
			issueId,
			"Test Issue",
			"Description",
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);
		_factory.IssueRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<IssueDto>.Ok(issueDto));
		_factory.IssueRepository
			.ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var response = await _client.DeleteAsync($"/api/v1/issues/{issueId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}
}
