// Copyright (c) 2026. All rights reserved.
// File Name :     PaginatedResponseTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

using Shared.DTOs;

namespace Tests.Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="PaginatedResponse{T}"/>.
/// </summary>
public sealed class PaginatedResponseTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var response = PaginatedResponse<string>.Empty;

		// Assert
		response.Items.Should().BeEmpty();
		response.Total.Should().Be(0);
		response.Page.Should().Be(1);
		response.PageSize.Should().Be(10);
		response.TotalPages.Should().Be(0);
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var items = new List<string> { "item1", "item2", "item3" };
		var total = 100L;
		var page = 2;
		var pageSize = 25;

		// Act
		var response = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response.Items.Should().BeEquivalentTo(items);
		response.Total.Should().Be(total);
		response.Page.Should().Be(page);
		response.PageSize.Should().Be(pageSize);
	}

	[Fact]
	public void TotalPages_CalculatesCorrectly_WhenTotalDivisibleByPageSize()
	{
		// Arrange
		var items = new List<string> { "item1", "item2" };
		var total = 100L;
		var page = 1;
		var pageSize = 25;

		// Act
		var response = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response.TotalPages.Should().Be(4);
	}

	[Fact]
	public void TotalPages_CalculatesCorrectly_WhenTotalNotDivisibleByPageSize()
	{
		// Arrange
		var items = new List<string> { "item1" };
		var total = 101L;
		var page = 1;
		var pageSize = 25;

		// Act
		var response = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response.TotalPages.Should().Be(5);
	}

	[Fact]
	public void TotalPages_ReturnsZero_WhenTotalIsZero()
	{
		// Arrange
		var items = Array.Empty<string>();
		var total = 0L;
		var page = 1;
		var pageSize = 10;

		// Act
		var response = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response.TotalPages.Should().Be(0);
	}

	[Fact]
	public void TotalPages_ReturnsOne_WhenTotalLessThanPageSize()
	{
		// Arrange
		var items = new List<string> { "item1", "item2" };
		var total = 5L;
		var page = 1;
		var pageSize = 10;

		// Act
		var response = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response.TotalPages.Should().Be(1);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var items = new List<string> { "item1", "item2" };
		var total = 100L;
		var page = 2;
		var pageSize = 25;

		// Act
		var response1 = new PaginatedResponse<string>(items, total, page, pageSize);
		var response2 = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response1.Should().Be(response2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var items = new List<string> { "item1", "item2" };
		var total = 100L;
		var page = 2;
		var pageSize = 25;

		// Act
		var response1 = new PaginatedResponse<string>(items, total, page, pageSize);
		var response2 = new PaginatedResponse<string>(items, total, page, pageSize);

		// Assert
		response1.Should().NotBeSameAs(response2);
	}
}
