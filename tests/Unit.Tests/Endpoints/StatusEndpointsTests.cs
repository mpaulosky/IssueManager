// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusEndpointsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Tests.Unit.Endpoints;

/// <summary>
/// Endpoint tests for Status API routes.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusEndpointsTests : IDisposable
{
	private readonly ApiWebApplicationFactory _factory;
	private readonly HttpClient _client;
	private readonly HttpClient _authenticatedClient;

	public StatusEndpointsTests()
	{
		_factory = new ApiWebApplicationFactory();
		_client = _factory.CreateClient();
		_authenticatedClient = _factory.CreateAuthenticatedClient();
	}

	public void Dispose()
	{
		_authenticatedClient.Dispose();
		_client.Dispose();
		_factory.Dispose();
	}

	[Fact]
	public async Task ListStatuses_ReturnsOk()
	{
		// Arrange
		IReadOnlyList<StatusDto> items = [];
		_factory.StatusRepository
			.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<StatusDto>>.Ok(items));

		// Act
		var response = await _client.GetAsync("/api/v1/statuses");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetStatus_WithValidId_ReturnsOk_WhenStatusExists()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var statusDto = new StatusDto(
			id,
			"Test Status",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.StatusRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(statusDto));

		// Act
		var response = await _client.GetAsync($"/api/v1/statuses/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task GetStatus_WithValidId_ReturnsNotFound_WhenStatusDoesNotExist()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_factory.StatusRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		// Act
		var response = await _client.GetAsync($"/api/v1/statuses/{id}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetStatus_WithInvalidId_ReturnsNotFound()
	{
		// Act
		var response = await _client.GetAsync("/api/v1/statuses/not-a-valid-id");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task CreateStatus_WithValidCommand_ReturnsCreated()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var statusDto = new StatusDto(
			statusId,
			"Test Status",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.StatusRepository
			.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(statusDto));

		var command = new { StatusName = "Test Status", StatusDescription = "Description" };

		// Act
		var response = await _authenticatedClient.PostAsJsonAsync("/api/v1/statuses", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
	}

	[Fact]
	public async Task CreateStatus_WithoutAuthentication_ReturnsUnauthorized()
	{
		// Arrange
		var command = new { StatusName = "Test Status", StatusDescription = "Description" };

		// Act
		var response = await _client.PostAsJsonAsync("/api/v1/statuses", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task UpdateStatus_WithValidCommand_ReturnsOk()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var statusDto = new StatusDto(
			statusId,
			"Updated Status",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.StatusRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(statusDto));
		_factory.StatusRepository
			.UpdateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(statusDto));

		var command = new { StatusName = "Updated Status", StatusDescription = "Description" };

		// Act
		var response = await _authenticatedClient.PatchAsJsonAsync($"/api/v1/statuses/{statusId}", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task UpdateStatus_WithoutAuthentication_ReturnsUnauthorized()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var command = new { StatusName = "Updated Status", StatusDescription = "Description" };

		// Act
		var response = await _client.PatchAsJsonAsync($"/api/v1/statuses/{statusId}", command);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task DeleteStatus_WithValidId_ReturnsNoContent()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var statusDto = new StatusDto(
			statusId,
			"Test Status",
			"Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
		_factory.StatusRepository
			.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(statusDto));
		_factory.StatusRepository
			.ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var response = await _authenticatedClient.DeleteAsync($"/api/v1/statuses/{statusId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task DeleteStatus_WithoutAuthentication_ReturnsUnauthorized()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();

		// Act
		var response = await _client.DeleteAsync($"/api/v1/statuses/{statusId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
	}
}
