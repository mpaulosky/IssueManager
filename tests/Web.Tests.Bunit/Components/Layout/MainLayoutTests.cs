// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MainLayoutTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Layout;

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
		TestContext.AddAuthorization();
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
		TestContext.AddAuthorization();
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
		TestContext.AddAuthorization();
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
		TestContext.AddAuthorization();
		TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

		// Act
		var cut = TestContext.Render<MainLayout>(parameters => parameters
			.Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Test"))));

		// Assert
		cut.Find("footer").Should().NotBeNull();
	}
}
