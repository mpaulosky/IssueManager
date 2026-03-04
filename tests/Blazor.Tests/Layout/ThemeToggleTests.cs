// Copyright (c) 2026. All rights reserved.

using BlazorTests.Fixtures;

namespace BlazorTests.Layout;

/// <summary>
/// bUnit tests for the <see cref="ThemeToggle"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeToggleTests : ComponentTestBase
{
	[Fact]
	public void ThemeToggle_RendersWithoutError()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<ThemeToggle>();

		// Assert
		cut.Should().NotBeNull();
		cut.Find("button").Should().NotBeNull();
	}

	[Fact]
	public void ThemeToggle_ShowsMoonIcon_WhenLightMode()
	{
		// Arrange
		TestContext.JSInterop.Setup<bool>("themeHelpers.isDark").SetResult(false);
		var cut = TestContext.Render<ThemeToggle>();

		// Act — wait for OnAfterRenderAsync
		cut.WaitForState(() => cut.Markup.Contains("M20.354"), timeout: TimeSpan.FromSeconds(2));

		// Assert — a moon icon path should be present
		cut.Markup.Should().Contain("M20.354");
	}

	[Fact]
	public void ThemeToggle_ShowsSunIcon_AfterToggle()
	{
		// Arrange
		TestContext.JSInterop.Setup<bool>("themeHelpers.isDark").SetResult(false);
		TestContext.JSInterop.SetupVoid("themeHelpers.setDark", true);
		var cut = TestContext.Render<ThemeToggle>();
		cut.WaitForState(() => cut.Markup.Contains("M20.354"), timeout: TimeSpan.FromSeconds(2));

		// Act — click the toggle button
		var button = cut.Find("button");
		button.Click();

		// Assert — a sun icon path should now be present
		cut.Markup.Should().Contain("M12 3v1m0 16v1m9-9h-1M4 12H3");
	}

	[Fact]
	public void ThemeToggle_HasAriaLabel()
	{
		// Arrange
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<ThemeToggle>();

		// Assert
		var button = cut.Find("button");
		button.GetAttribute("aria-label").Should().NotBeNullOrEmpty();
	}
}
