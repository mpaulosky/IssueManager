// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     TokenForwardingHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using Microsoft.AspNetCore.Authentication;

namespace BlazorTests.Services;

/// <summary>
/// Unit tests for the <see cref="TokenForwardingHandler"/> DelegatingHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class TokenForwardingHandlerTests
{
	[Fact]
	public async Task SendAsync_WithAccessToken_AddsBearerHeader()
	{
		// Arrange
		const string accessToken = "test-access-token";
		var context = new DefaultHttpContext();
		var authService = Substitute.For<IAuthenticationService>();

		// GetTokenAsync is an extension method — cannot be mocked directly.
		// Store the token in AuthenticationProperties so the real extension reads it.
		var authProperties = new AuthenticationProperties();
		authProperties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = accessToken }]);
		authService.AuthenticateAsync(context, null)
			.Returns(AuthenticateResult.Success(new AuthenticationTicket(
				new System.Security.Claims.ClaimsPrincipal(),
				authProperties,
				"TestScheme")));

		context.RequestServices = new ServiceCollection()
			.AddSingleton(authService)
			.BuildServiceProvider();

		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		var innerHandler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request, Xunit.TestContext.Current.CancellationToken);

		// Assert
		response.Should().NotBeNull();
		innerHandler.LastRequest.Should().NotBeNull();
		innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
		innerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
		innerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be(accessToken);
	}

	[Fact]
	public async Task SendAsync_WithoutToken_DoesNotAddHeader()
	{
		// Arrange
		var context = new DefaultHttpContext();
		var authService = Substitute.For<IAuthenticationService>();
		// No token stored → GetTokenAsync returns null via real extension method logic
		authService.AuthenticateAsync(context, null)
			.Returns(AuthenticateResult.Success(new AuthenticationTicket(
				new System.Security.Claims.ClaimsPrincipal(),
				new AuthenticationProperties(),
				"TestScheme")));
		context.RequestServices = new ServiceCollection()
			.AddSingleton(authService)
			.BuildServiceProvider();

		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		var innerHandler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request, Xunit.TestContext.Current.CancellationToken);

		// Assert
		response.Should().NotBeNull();
		innerHandler.LastRequest.Should().NotBeNull();
		innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
	}

	[Fact]
	public async Task SendAsync_WithNullContext_DoesNotAddHeader()
	{
		// Arrange
		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns((HttpContext)null!);

		var innerHandler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request, Xunit.TestContext.Current.CancellationToken);

		// Assert
		response.Should().NotBeNull();
		innerHandler.LastRequest.Should().NotBeNull();
		innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
	}

	[Fact]
	public async Task SendAsync_ForwardsToInnerHandler()
	{
		// Arrange
		var context = new DefaultHttpContext();
		var authService = Substitute.For<IAuthenticationService>();
		authService.AuthenticateAsync(context, null)
			.Returns(AuthenticateResult.Fail("no token"));
		context.RequestServices = new ServiceCollection()
			.AddSingleton(authService)
			.BuildServiceProvider();
		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		var innerHandler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request, Xunit.TestContext.Current.CancellationToken);

		// Assert
		response.Should().NotBeNull();
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		innerHandler.LastRequest.Should().NotBeNull();
		innerHandler.LastRequest!.RequestUri.Should().Be(new Uri("http://test.local/api"));
	}

	/// <summary>
	/// Test helper that captures the last request passed through.
	/// </summary>
	private sealed class TestHttpMessageHandler : HttpMessageHandler
	{
		public HttpRequestMessage? LastRequest { get; private set; }
		private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

		public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
			=> _handler = handler;

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
		{
			LastRequest = request;
			return Task.FromResult(_handler(request));
		}
	}
}
