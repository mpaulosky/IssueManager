// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ThemeColorSelectorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

using Microsoft.JSInterop;

namespace Web.Components.Layout;

/// <summary>
/// bUnit tests for the <see cref="ThemeColorSelector"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeColorSelectorTests : ComponentTestBase
{
	[Fact]
	public void ThemeColorSelector_RendersWithoutError()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<ThemeColorSelector>();

		// Assert
		cut.Should().NotBeNull();
		cut.Find("button[aria-label='Change color theme']").Should().NotBeNull();
	}

	[Fact]
	public void ThemeColorSelector_ShowsDropdown_OnButtonClick()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
		var cut = TestContext.Render<ThemeColorSelector>();

		// Act — click the button to toggle dropdown
		var button = cut.Find("button[aria-label='Change color theme']");
		button.Click();

		// Assert — a dropdown menu should now be visible
		var menu = cut.Find("[role='menu']");
		menu.Should().NotBeNull();
		cut.Markup.Should().Contain("Blue");
		cut.Markup.Should().Contain("Green");
		cut.Markup.Should().Contain("Red");
		cut.Markup.Should().Contain("Yellow");
	}

	[Fact]
	public void ThemeColorSelector_SelectTheme_ClosesDropdownAndSetsTheme()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
		var cut = TestContext.Render<ThemeColorSelector>();

		// Open dropdown first
		cut.Find("button[aria-label='Change color theme']").Click();
		cut.Find("[role='menu']").Should().NotBeNull();

		// Act — click the "Green" theme menu item
		var menuItems = cut.FindAll("button[role='menuitem']");
		var greenItem = menuItems.First(b => b.TextContent.Contains("Green"));
		greenItem.Click();

		// Assert — dropdown is closed after selecting a theme
		cut.FindAll("[role='menu']").Should().BeEmpty();
	}

	[Fact]
	public void ThemeColorSelector_HandleFocusOut_ClosesOpenDropdown()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
		var cut = TestContext.Render<ThemeColorSelector>();

		// Open dropdown first
		cut.Find("button[aria-label='Change color theme']").Click();
		cut.Find("[role='menu']").Should().NotBeNull();

		// Act — trigger focusout on the wrapper div to invoke HandleFocusOut
		cut.Find("div.relative").TriggerEvent("onfocusout", new FocusEventArgs());

		// Assert — dropdown is now closed
		cut.FindAll("[role='menu']").Should().BeEmpty();
	}

	[Fact]
	public void ThemeColorSelector_OnAfterRenderAsync_WhenJSThrows_RendersWithoutError()
	{
		// Arrange — configure JS to throw so the catch block in OnAfterRenderAsync is exercised
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
		TestContext.JSInterop.Setup<string>("themeHelpers.getColorTheme")
			.SetException(new JSException("JS interop unavailable"));

		// Act — OnAfterRenderAsync catches the JSException and defaults to blue
		var cut = TestContext.Render<ThemeColorSelector>();

		// Assert — component renders successfully despite the JS failure
		cut.Should().NotBeNull();
		cut.Find("button[aria-label='Change color theme']").Should().NotBeNull();
	}
}
