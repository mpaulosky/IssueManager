// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     PaginationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Blazor.Tests
// =============================================

using BlazorTests.Fixtures;

namespace BlazorTests.Components.Shared;

/// <summary>
/// Unit tests for the <see cref="Pagination"/> Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class PaginationTests : ComponentTestBase
{
	[Fact]
	public void Pagination_DoesNotRender_WhenTotalPagesIsOne()
	{
		// Act
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 1)
			.Add(x => x.TotalPages, 1));

		// Assert
		cut.FindAll("nav").Should().BeEmpty();
	}

	[Fact]
	public void Pagination_DoesNotRender_WhenTotalPagesIsZero()
	{
		// Act
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 1)
			.Add(x => x.TotalPages, 0));

		// Assert
		cut.FindAll("nav").Should().BeEmpty();
	}

	[Fact]
	public void Pagination_RendersPageButtons_WhenTotalPagesIsGreaterThanOne()
	{
		// Act
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 1)
			.Add(x => x.TotalPages, 3));

		// Assert
		cut.Find("nav").Should().NotBeNull();
		cut.FindAll("button").Should().HaveCountGreaterThan(0);
	}

	[Fact]
	public void Pagination_PreviousButtonIsDisabled_OnFirstPage()
	{
		// Act
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 1)
			.Add(x => x.TotalPages, 5));

		// Assert
		var prevButton = cut.Find("button[aria-label='Previous page']");
		prevButton.HasAttribute("disabled").Should().BeTrue();
	}

	[Fact]
	public void Pagination_NextButtonIsDisabled_OnLastPage()
	{
		// Act
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 5)
			.Add(x => x.TotalPages, 5));

		// Assert
		var nextButton = cut.Find("button[aria-label='Next page']");
		nextButton.HasAttribute("disabled").Should().BeTrue();
	}

	[Fact]
	public async Task Pagination_InvokesOnPageChanged_WhenPageButtonClicked()
	{
		// Arrange
		var changedPage = 0;
		var cut = TestContext.Render<Pagination>(p => p
			.Add(x => x.CurrentPage, 1)
			.Add(x => x.TotalPages, 5)
			.Add(x => x.OnPageChanged, EventCallback.Factory.Create<int>(this, page => changedPage = page)));

		// Act
		var pageButton = cut.Find("button[aria-label='Page 2']");
		await pageButton.ClickAsync(new MouseEventArgs());

		// Assert
		changedPage.Should().Be(2);
	}
}
