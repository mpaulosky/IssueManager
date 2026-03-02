// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace Tests.BlazorTests.Layout;

/// <summary>
/// bUnit tests for the <see cref="MainLayout"/> LayoutComponentBase.
/// </summary>
[ExcludeFromCodeCoverage]
public class MainLayoutTests : ComponentTestBase
{
	[Fact]
	public void MainLayout_RendersWithoutError()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<MainLayout>(parameters => parameters
			.Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Test Body"))));

		// Assert
		cut.Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_RendersBodyContent()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
		const string bodyContent = "Test Body Content";

		// Act
		var cut = TestContext.Render<MainLayout>(parameters => parameters
			.Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, bodyContent))));

		// Assert
		cut.Markup.Should().Contain(bodyContent);
	}

	[Fact]
	public void MainLayout_ContainsNavMenu()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<MainLayout>(parameters => parameters
			.Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Test"))));

		// Assert
		cut.Find("nav").Should().NotBeNull();
	}

	[Fact]
	public void MainLayout_ContainsFooter()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<MainLayout>(parameters => parameters
			.Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Test"))));

		// Assert
		cut.Find("footer").Should().NotBeNull();
	}

	private static AuthenticationStateProvider CreateTestAuthorizationContext(bool isAuthorized, string userName = "TestUser")
	{
		var identity = isAuthorized
			? new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "Test")
			: new ClaimsIdentity();

		var user = new ClaimsPrincipal(identity);
		var authState = Task.FromResult(new AuthenticationState(user));

		var authStateProvider = Substitute.For<AuthenticationStateProvider>();
		authStateProvider.GetAuthenticationStateAsync().Returns(authState);

		return authStateProvider;
	}
}
