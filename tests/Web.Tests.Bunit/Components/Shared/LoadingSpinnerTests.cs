// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     LoadingSpinnerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for the <see cref="LoadingSpinner"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoadingSpinnerTests : ComponentTestBase
{
	[Fact]
	public void LoadingSpinner_ShowsDefaultLabel_WhenLabelNotProvided()
	{
		// Act
		var cut = TestContext.Render<LoadingSpinner>();

		// Assert
		cut.Find("span").TextContent.Should().Be("Loading...");
	}

	[Fact]
	public void LoadingSpinner_ShowsCustomLabel_WhenLabelIsProvided()
	{
		// Act
		var cut = TestContext.Render<LoadingSpinner>(p =>
			p.Add(x => x.Label, "Please wait"));

		// Assert
		cut.Find("span").TextContent.Should().Be("Please wait");
	}

	[Fact]
	public void LoadingSpinner_HasAnimateSpinClass_InMarkup()
	{
		// Act
		var cut = TestContext.Render<LoadingSpinner>();

		// Assert
		cut.Find(".animate-spin").Should().NotBeNull();
	}
}
