// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryApiClientTests.cs
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
/// Unit tests for the <see cref="CategoryApiClient"/> HTTP client.
/// </summary>
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
		var result = await client.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsEmpty_OnNonSuccessResponse()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
		var client = new CategoryApiClient(CreateMockClient(response));

		// Act
		var result = await client.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
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
		var result = await client.CreateAsync(command);

		// Assert
		result.Should().NotBeNull();
		result!.CategoryName.Should().Be("New Category");
	}

	[Fact]
	public async Task CreateAsync_ReturnsNull_OnBadRequest()
	{
		// Arrange
		var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var client = new CategoryApiClient(CreateMockClient(response));
		var command = new CreateCategoryCommand { CategoryName = "x" };

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
		var client = new CategoryApiClient(CreateMockClient(response));

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
		var client = new CategoryApiClient(CreateMockClient(response));

		// Act
		var result = await client.DeleteAsync(ObjectId.GenerateNewId().ToString());

		// Assert
		result.Should().BeFalse();
	}
}
