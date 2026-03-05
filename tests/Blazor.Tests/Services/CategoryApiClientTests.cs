// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryApiClientTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

namespace BlazorTests.Services;

/// <summary>
/// Unit tests for the <see cref="CategoryApiClient"/> HTTP client.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryApiClientTests
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

	private static CategoryDto MakeCategory(string name = "Test Category") => new(
		ObjectId.GenerateNewId(),
		name,
		"Test Description",
		DateTime.UtcNow,
		null,
		false,
		UserDto.Empty);

	[Fact]
	public async Task GetAllAsync_ReturnsCategories_OnSuccess()
	{
		// Arrange
		var categories = new List<CategoryDto> { MakeCategory("Bug"), MakeCategory("Feature") };
		var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(categories) };
		var client = new CategoryApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CategoryDto> categoryDtos = result as CategoryDto[] ?? result.ToArray();
		categoryDtos.Should().NotBeNull();
		categoryDtos.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsEmpty_OnNonSuccessResponse()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
		var client = new CategoryApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync(Xunit.TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CategoryDto> categoryDtos = result as CategoryDto[] ?? result.ToArray();
		categoryDtos.Should().NotBeNull();
		categoryDtos.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateAsync_ReturnsCategoryDto_OnCreated()
	{
		// Arrange
		var expected = MakeCategory("New Category");
		var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(expected) };
		var client = new CategoryApiClient(CreateMockClient(response));
		var command = new CreateCategoryCommand { CategoryName = "New Category" };

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be("New Category");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new CategoryApiClient(CreateMockClient(response));
		var command = new CreateCategoryCommand { CategoryName = "x" };

		// Act
		var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

}
