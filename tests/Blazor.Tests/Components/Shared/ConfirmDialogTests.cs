// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ConfirmDialogTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Tests.BlazorTests.Fixtures;

using Web.Components.Shared;

namespace Tests.BlazorTests.Components.Shared;

/// <summary>
/// Unit tests for the <see cref="ConfirmDialog"/> Blazor component.
/// </summary>
public class ConfirmDialogTests : ComponentTestBase
{
	[Fact]
	public void ConfirmDialog_DoesNotRender_WhenIsVisibleIsFalse()
	{
		// Act
		var cut = TestContext.Render<ConfirmDialog>(p =>
			p.Add(x => x.IsVisible, false));

		// Assert
		cut.FindAll("[role='dialog']").Should().BeEmpty();
	}

	[Fact]
	public void ConfirmDialog_Renders_WhenIsVisibleIsTrue()
	{
		// Act
		var cut = TestContext.Render<ConfirmDialog>(p =>
			p.Add(x => x.IsVisible, true));

		// Assert
		cut.Find("[role='dialog']").Should().NotBeNull();
	}

	[Fact]
	public void ConfirmDialog_RendersTitleAndMessage_WhenVisible()
	{
		// Act
		var cut = TestContext.Render<ConfirmDialog>(p => p
			.Add(x => x.IsVisible, true)
			.Add(x => x.Title, "Delete Item")
			.Add(x => x.Message, "Are you sure?"));

		// Assert
		cut.Find("#confirm-dialog-title").TextContent.Trim().Should().Be("Delete Item");
		cut.Find("#confirm-dialog-message").TextContent.Trim().Should().Be("Are you sure?");
	}

	[Fact]
	public async Task ConfirmDialog_InvokesOnConfirm_WhenConfirmButtonClicked()
	{
		// Arrange
		var confirmed = false;
		var cut = TestContext.Render<ConfirmDialog>(p => p
			.Add(x => x.IsVisible, true)
			.Add(x => x.ConfirmText, "Yes, Delete")
			.Add(x => x.OnConfirm, EventCallback.Factory.Create(this, () => { confirmed = true; })));

		// Act
		var confirmButton = cut.FindAll("button")
			.First(b => b.TextContent.Contains("Yes, Delete"));
		await confirmButton.ClickAsync(new MouseEventArgs());

		// Assert
		confirmed.Should().BeTrue();
	}

	[Fact]
	public async Task ConfirmDialog_InvokesOnCancel_WhenCancelButtonClicked()
	{
		// Arrange
		var cancelled = false;
		var cut = TestContext.Render<ConfirmDialog>(p => p
			.Add(x => x.IsVisible, true)
			.Add(x => x.CancelText, "No, Keep")
			.Add(x => x.OnCancel, EventCallback.Factory.Create(this, () => { cancelled = true; })));

		// Act
		var cancelButton = cut.FindAll("button")
			.First(b => b.TextContent.Contains("No, Keep"));
		await cancelButton.ClickAsync(new MouseEventArgs());

		// Assert
		cancelled.Should().BeTrue();
	}
}
