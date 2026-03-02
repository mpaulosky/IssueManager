// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Tests.BlazorTests.Layout;

/// <summary>
/// bUnit tests for the <see cref="NavMenu"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class NavMenuTests : ComponentTestBase
{
	[Fact]
	public void NavMenu_RendersWithoutError()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert
		cut.Should().NotBeNull();
		cut.Find("nav").Should().NotBeNull();
	}

	[Fact]
	public void NavMenu_ShowsNavLinks()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("href=\"/\"");
		cut.Markup.Should().Contain("href=\"/issues\"");
		cut.Markup.Should().Contain("href=\"/categories\"");
		cut.Markup.Should().Contain("href=\"/statuses\"");
	}

	[Fact]
	public void NavMenu_ShowsLoginLink_WhenNotAuthorized()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("Log in");
		cut.Markup.Should().Contain("href=\"/auth/login\"");
	}

	[Fact]
	public void NavMenu_ShowsLogoutLink_WhenAuthorized()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: true, userName: "TestUser");
		TestContext.Services.AddSingleton(authContext);

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert
		cut.Markup.Should().Contain("Log out");
		cut.Markup.Should().Contain("href=\"/auth/logout\"");
		cut.Markup.Should().Contain("Hello, TestUser!");
	}

	[Fact]
	public void NavMenu_MobileMenu_TogglesOnButtonClick()
	{
		// Arrange
		var authContext = CreateTestAuthorizationContext(isAuthorized: false);
		TestContext.Services.AddSingleton(authContext);
		var cut = TestContext.Render<NavMenu>();

		// Mobile menu should not be visible initially
		var mobileMenus = cut.FindAll("#mobile-menu");
		mobileMenus.Should().BeEmpty();

		// Act — click the hamburger button
		var hamburgerButton = cut.Find("button[aria-controls='mobile-menu']");
		hamburgerButton.Click();

		// Assert — mobile menu should now be visible
		var mobileMenu = cut.Find("#mobile-menu");
		mobileMenu.Should().NotBeNull();
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
