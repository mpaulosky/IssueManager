// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueApiClientTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using System.Net;
using System.Net.Http.Json;

using MongoDB.Bson;

using Shared.DTOs;
using Shared.Validators;

using Web.Services;

namespace Tests.BlazorTests.Services;

/// <summary>
/// Unit tests for the <see cref="IssueApiClient"/> HTTP client.
/// </summary>
public class IssueApiClientTests
{
	private static HttpClient CreateMockClient(HttpResponseMessage response, string baseUrl = "https://api.test")
	{
		var handler = new MockHandler(response);
		return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
	}

	private sealed class MockHandler : HttpMessageHandler
	{
		private readonly HttpResponseMessage _response;
		public MockHandler(HttpResponseMessage response) => _response = response;
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> Task.FromResult(_response);
	}

	private static IssueDto MakeIssue(string title = "Test Issue") => new(
		ObjectId.GenerateNewId(),
		title,
		"Test Description",
		DateTime.UtcNow,
		null,
		new UserDto("id", "Name", "test@test.com"),
		CategoryDto.Empty,
		new StatusDto(ObjectId.GenerateNewId(), "Open", "Open status", DateTime.UtcNow, null, false, UserDto.Empty),
		false,
		UserDto.Empty,
		false,
		false);

	[Fact]
	public async Task GetAllAsync_ReturnsPaginatedResponse_OnSuccess()
	{
		// Arrange
		var issue = MakeIssue();
		var expected = new PaginatedResponse<IssueDto>([issue], 1, 1, 20);
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Items.Should().HaveCount(1);
		result.Total.Should().Be(1L);
		result.Page.Should().Be(1);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsEmpty_OnNonSuccessResponse()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Items.Should().BeEmpty();
		result.Total.Should().Be(0L);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsIssueDto_OnSuccess()
	{
		// Arrange
		var expected = MakeIssue("Specific Issue");
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetByIdAsync(expected.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result!.Title.Should().Be("Specific Issue");
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_OnNotFound()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.NotFound);
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetByIdAsync(ObjectId.GenerateNewId().ToString());

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task CreateAsync_ReturnsIssueDto_OnCreated()
	{
		// Arrange
		var expected = MakeIssue("New Issue");
		var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(expected) };
		var client = new IssueApiClient(CreateMockClient(response));
		var command = new CreateIssueCommand { Title = "New Issue", Description = "Description" };

		// Act
		var result = await client.CreateAsync(command);

		// Assert
		result.Should().NotBeNull();
		result!.Title.Should().Be("New Issue");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new IssueApiClient(CreateMockClient(response));
		var command = new CreateIssueCommand { Title = "x" };

		// Act
		var result = await client.CreateAsync(command);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_ReturnsTrue_OnNoContent()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.NoContent);
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString());

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task DeleteAsync_ReturnsFalse_OnNotFound()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.NotFound);
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString());

		// Assert
		result.Should().BeFalse();
	}
}
