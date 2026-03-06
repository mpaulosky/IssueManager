// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     NavMenuTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Layout;

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
		TestContext.AddAuthorization();
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

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
		TestContext.AddAuthorization();
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert — Home and Issues are always visible
		cut.Markup.Should().Contain("href=\"/\"");
		cut.Markup.Should().Contain("href=\"/issues\"");
	}

	[Fact]
	public void NavMenu_ShowsAdminLinks_WhenAdminRole()
	{
		// Arrange
		var authContext = TestContext.AddAuthorization();
		authContext.SetAuthorized("AdminUser");
		authContext.SetRoles("Admin");
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<NavMenu>();

		// Assert — admin-only links visible when the user is Admin
		cut.Markup.Should().Contain("href=\"/categories\"");
		cut.Markup.Should().Contain("href=\"/statuses\"");
	}

	[Fact]
	public void NavMenu_ShowsLoginLink_WhenNotAuthorized()
	{
		// Arrange
		TestContext.AddAuthorization();
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

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
		var authContext = TestContext.AddAuthorization();
		authContext.SetAuthorized("TestUser");
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

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
		TestContext.AddAuthorization();
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
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
}
