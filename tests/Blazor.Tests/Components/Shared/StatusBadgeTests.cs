// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusBadgeTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using BlazorTests.Fixtures;

namespace BlazorTests.Components.Shared;

/// <summary>
/// Unit tests for the <see cref="StatusBadge"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusBadgeTests : ComponentTestBase
{
	private static StatusDto MakeStatus(string name) =>
		new(ObjectId.GenerateNewId(), name, $"{name} status", DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public void StatusBadge_RendersGreenBadge_ForOpenStatus()
	{
		// Act
		var cut = TestContext.Render<StatusBadge>(p =>
			p.Add(x => x.Status, MakeStatus("Open")));

		// Assert
		cut.Find("span").ClassList.Should().Contain("bg-green-100");
	}

	[Fact]
	public void StatusBadge_RendersBlueBadge_ForInProgressStatus()
	{
		// Act
		var cut = TestContext.Render<StatusBadge>(p =>
			p.Add(x => x.Status, MakeStatus("In Progress")));

		// Assert
		cut.Find("span").ClassList.Should().Contain("bg-blue-100");
	}

	[Fact]
	public void StatusBadge_RendersGrayBadge_ForClosedStatus()
	{
		// Act
		var cut = TestContext.Render<StatusBadge>(p =>
			p.Add(x => x.Status, MakeStatus("Closed")));

		// Assert
		cut.Find("span").ClassList.Should().Contain("bg-gray-100");
	}

	[Fact]
	public void StatusBadge_RendersPurpleBadge_ForUnknownStatus()
	{
		// Act
		var cut = TestContext.Render<StatusBadge>(p =>
			p.Add(x => x.Status, MakeStatus("Unknown")));

		// Assert
		cut.Find("span").ClassList.Should().Contain("bg-purple-100");
	}

	[Fact]
	public void StatusBadge_RendersStatusName_InBadgeText()
	{
		// Arrange
		var status = MakeStatus("Open");

		// Act
		var cut = TestContext.Render<StatusBadge>(p =>
			p.Add(x => x.Status, status));

		// Assert
		cut.Find("span").TextContent.Should().Be("Open");
	}
}
