// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ThemeColorSelectorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using BlazorTests.Fixtures;

namespace BlazorTests.Layout;

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
}
