// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentApiClientTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Comments;

/// <summary>
/// Unit tests for the <see cref="CommentApiClient"/> HTTP client.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommentApiClientTests
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

	private static CommentDto MakeComment(string title = "Test Comment") => new(
		ObjectId.GenerateNewId(),
		title,
		"Test comment text",
		DateTime.UtcNow,
		null,
		IssueDto.Empty,
		new UserDto("id", "Name", "test@test.com"),
		[],
		false,
		UserDto.Empty,
		false,
		UserDto.Empty);

	[Fact]
	public async Task GetAllAsync_ReturnsComments_OnSuccess()
	{
		// Arrange
		var comments = new List<CommentDto> { MakeComment("First"), MakeComment("Second") };
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(comments) };
		var client = new CommentApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CommentDto> commentDtos = result as CommentDto[] ?? result.ToArray();
		commentDtos.Should().NotBeNull();
		commentDtos.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsEmpty_OnNonSuccessResponse()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
		var client = new CommentApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CommentDto> commentDtos = result as CommentDto[] ?? result.ToArray();
		commentDtos.Should().NotBeNull();
		commentDtos.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateAsync_ReturnsCommentDto_OnCreated()
	{
		// Arrange
		var expected = MakeComment("New Comment");
		var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(expected) };
		var client = new CommentApiClient(CreateMockClient(response));
		var command = new CreateCommentCommand
		{
			Title = "New Comment",
			CommentText = "Some comment text",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("New Comment");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new CommentApiClient(CreateMockClient(response));
		var command = new CreateCommentCommand { Title = "x", CommentText = "", IssueId = "invalid" };

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
		var client = new CommentApiClient(CreateMockClient(response));

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
		var client = new CommentApiClient(CreateMockClient(response));

		// Act
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString(), Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeFalse();
	}
}
