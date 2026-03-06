// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueApiClientTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Issues;

/// <summary>
/// Unit tests for the <see cref="IssueApiClient"/> HTTP client.
/// </summary>
[ExcludeFromCodeCoverage]
public class IssueApiClientTests
{
	private static HttpClient CreateMockClient(HttpResponseMessage response, string baseUrl = "https://api.test")
	{
		var handler = new MockHandler(response);
		return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
	}

	private static (HttpClient client, MockHandler handler) CreateMockClientWithHandler(HttpResponseMessage response, string baseUrl = "https://api.test")
	{
		var handler = new MockHandler(response);
		var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
		return (client, handler);
	}

	private sealed class MockHandler : HttpMessageHandler
	{
		private readonly HttpResponseMessage _response;
		public HttpRequestMessage? LastRequest { get; private set; }
		public MockHandler(HttpResponseMessage response) => _response = response;
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			LastRequest = request;
			return Task.FromResult(_response);
		}
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
		var result = await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

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
		var result = await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

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
		var result = await client.GetByIdAsync(expected.Id.ToString(), Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Specific Issue");
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_OnNotFound()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.NotFound);
		var client = new IssueApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetByIdAsync(ObjectId.GenerateNewId().ToString(), Xunit.TestContext.Current.CancellationToken);

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
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("New Issue");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new IssueApiClient(CreateMockClient(response));
		var command = new CreateIssueCommand { Title = "x" };

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

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
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString(), Xunit.TestContext.Current.CancellationToken);

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
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString(), Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public async Task GetAllAsync_WithSearchTerm_IncludesSearchTermInUrl()
	{
		// Arrange
		var issue = MakeIssue();
		var expected = new PaginatedResponse<IssueDto>([issue], 1, 1, 20);
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var (httpClient, handler) = CreateMockClientWithHandler(response);
		var client = new IssueApiClient(httpClient);

		// Act
		await client.GetAllAsync(searchTerm: "bug fix", cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		handler.LastRequest.Should().NotBeNull();
		handler.LastRequest!.RequestUri.Should().NotBeNull();
		handler.LastRequest.RequestUri!.Query.Should().Contain("searchTerm=bug%20fix");
	}

	[Fact]
	public async Task GetAllAsync_WithAuthorName_IncludesAuthorNameInUrl()
	{
		// Arrange
		var issue = MakeIssue();
		var expected = new PaginatedResponse<IssueDto>([issue], 1, 1, 20);
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var (httpClient, handler) = CreateMockClientWithHandler(response);
		var client = new IssueApiClient(httpClient);

		// Act
		await client.GetAllAsync(authorName: "John", cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		handler.LastRequest.Should().NotBeNull();
		handler.LastRequest!.RequestUri.Should().NotBeNull();
		handler.LastRequest.RequestUri!.Query.Should().Contain("authorName=John");
	}

	[Fact]
	public async Task GetAllAsync_WithBothFilters_IncludesBothInUrl()
	{
		// Arrange
		var issue = MakeIssue();
		var expected = new PaginatedResponse<IssueDto>([issue], 1, 1, 20);
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var (httpClient, handler) = CreateMockClientWithHandler(response);
		var client = new IssueApiClient(httpClient);

		// Act
		await client.GetAllAsync(searchTerm: "bug", authorName: "Alice", cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		handler.LastRequest.Should().NotBeNull();
		handler.LastRequest!.RequestUri.Should().NotBeNull();
		var query = handler.LastRequest.RequestUri!.Query;
		query.Should().Contain("searchTerm=bug");
		query.Should().Contain("authorName=Alice");
	}

	[Fact]
	public async Task GetAllAsync_WithNullFilters_DoesNotIncludeFilterParams()
	{
		// Arrange
		var issue = MakeIssue();
		var expected = new PaginatedResponse<IssueDto>([issue], 1, 1, 20);
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) };
		var (httpClient, handler) = CreateMockClientWithHandler(response);
		var client = new IssueApiClient(httpClient);

		// Act
		await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		handler.LastRequest.Should().NotBeNull();
		handler.LastRequest!.RequestUri.Should().NotBeNull();
		var query = handler.LastRequest.RequestUri!.Query;
		query.Should().NotContain("searchTerm");
		query.Should().NotContain("authorName");
	}
}
