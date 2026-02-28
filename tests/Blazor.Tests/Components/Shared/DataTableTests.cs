// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DataTableTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Tests.BlazorTests.Fixtures;

using Web.Components.Shared;

namespace Tests.BlazorTests.Components.Shared;

/// <summary>
/// Unit tests for the <see cref="DataTable{TItem}"/> generic Blazor component.
/// </summary>
public class DataTableTests : ComponentTestBase
{
	private static RenderFragment<string> StringRowContent =>
		item => builder =>
		{
			builder.OpenElement(0, "tr");
			builder.OpenElement(1, "td");
			builder.AddContent(2, item);
			builder.CloseElement();
			builder.CloseElement();
		};

	[Fact]
	public void DataTable_ShowsLoadingSpinner_WhenIsLoadingIsTrue()
	{
		// Act
		var cut = TestContext.Render<DataTable<string>>(p => p
			.Add(x => x.Items, Array.Empty<string>())
			.Add(x => x.IsLoading, true));

		// Assert
		cut.FindComponent<LoadingSpinner>().Should().NotBeNull();
	}

	[Fact]
	public void DataTable_ShowsCustomEmptyMessage_WhenItemsIsEmpty()
	{
		// Act
		var cut = TestContext.Render<DataTable<string>>(p => p
			.Add(x => x.Items, Array.Empty<string>())
			.Add(x => x.EmptyMessage, "Nothing here yet"));

		// Assert
		cut.Markup.Should().Contain("Nothing here yet");
	}

	[Fact]
	public void DataTable_ShowsDefaultEmptyMessage_WhenEmptyMessageNotProvided_AndItemsEmpty()
	{
		// Act
		var cut = TestContext.Render<DataTable<string>>(p => p
			.Add(x => x.Items, Array.Empty<string>()));

		// Assert
		cut.Markup.Should().Contain("No items found.");
	}

	[Fact]
	public void DataTable_RendersTableRows_WhenItemsHasData()
	{
		// Arrange
		var items = new[] { "Alpha", "Beta" };

		// Act
		var cut = TestContext.Render<DataTable<string>>(p => p
			.Add(x => x.Items, items)
			.Add(x => x.RowContent, StringRowContent));

		// Assert — two <tr> rows in the table body
		cut.FindAll("tbody tr").Should().HaveCount(2);
		cut.Markup.Should().Contain("Alpha");
		cut.Markup.Should().Contain("Beta");
	}
}
