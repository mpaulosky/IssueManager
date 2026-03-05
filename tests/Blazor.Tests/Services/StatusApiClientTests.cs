// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusApiClientTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

namespace BlazorTests.Services;

/// <summary>
/// Unit tests for the <see cref="StatusApiClient"/> HTTP client.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusApiClientTests
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

	private static StatusDto MakeStatus(string name = "Open") => new(
		ObjectId.GenerateNewId(),
		name,
		$"{name} status",
		DateTime.UtcNow,
		null,
		false,
		UserDto.Empty);

	[Fact]
	public async Task GetAllAsync_ReturnsStatuses_OnSuccess()
	{
		// Arrange
		var statuses = new List<StatusDto> { MakeStatus(), MakeStatus("Closed") };
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(statuses) };
		var client = new StatusApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<StatusDto> statusDtos = result as StatusDto[] ?? result.ToArray();
		statusDtos.Should().NotBeNull();
		statusDtos.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsEmpty_OnNonSuccessResponse()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
		var client = new StatusApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<StatusDto> statusDtos = result as StatusDto[] ?? result.ToArray();
		statusDtos.Should().NotBeNull();
		statusDtos.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateAsync_ReturnsStatusDto_OnCreated()
	{
		// Arrange
		var expected = MakeStatus("In Progress");
		var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(expected) };
		var client = new StatusApiClient(CreateMockClient(response));
		var command = new CreateStatusCommand { StatusName = "In Progress" };

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.StatusName.Should().Be("In Progress");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new StatusApiClient(CreateMockClient(response));
		var command = new CreateStatusCommand { StatusName = "x" };

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

}
