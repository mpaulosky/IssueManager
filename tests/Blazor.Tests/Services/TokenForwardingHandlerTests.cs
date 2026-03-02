// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Tests.BlazorTests.Services;

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
		authService.AuthenticateAsync(context, null)
			.Returns(AuthenticateResult.Success(new Microsoft.AspNetCore.Authentication.AuthenticationTicket(
				new System.Security.Claims.ClaimsPrincipal(),
				"TestScheme")));

		context.RequestServices = new ServiceCollection()
			.AddSingleton(authService)
			.BuildServiceProvider();

		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		// Mock GetTokenAsync by setting up the auth service
		authService.GetTokenAsync(context, "access_token")
			.Returns(Task.FromResult<string?>(accessToken));

		var innerHandler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request);

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
		context.RequestServices = new ServiceCollection()
			.AddSingleton(authService)
			.BuildServiceProvider();

		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		// Return null token
		authService.GetTokenAsync(context, "access_token")
			.Returns(Task.FromResult<string?>(null));

		var innerHandler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request);

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

		var innerHandler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request);

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
		var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		httpContextAccessor.HttpContext.Returns(context);

		var innerHandler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
		var handler = new TokenForwardingHandler(httpContextAccessor) { InnerHandler = innerHandler };
		using var client = new HttpClient(handler);
		var request = new HttpRequestMessage(HttpMethod.Get, "http://test.local/api");

		// Act
		var response = await client.SendAsync(request);

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
