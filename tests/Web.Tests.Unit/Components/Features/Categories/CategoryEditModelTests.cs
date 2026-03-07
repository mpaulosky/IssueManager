// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryEditModelTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Unit
// =============================================

namespace Web.Components.Features.Categories;

/// <summary>
/// Unit tests for <see cref="CategoryEditModel"/> default values and property setters.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryEditModelTests
{
	[Fact]
	public void Constructor_SetsExpectedDefaultValues()
	{
		// Arrange / Act
		var model = new CategoryEditModel();

		// Assert
		model.Id.Should().Be("");
		model.CategoryName.Should().Be("");
		model.CategoryDescription.Should().BeNull();
	}

	[Fact]
	public void Id_CanBeSet()
	{
		// Arrange
		var model = new CategoryEditModel();

		// Act
		model.Id = "xyz789";

		// Assert
		model.Id.Should().Be("xyz789");
	}

	[Fact]
	public void CategoryName_CanBeSet()
	{
		// Arrange
		var model = new CategoryEditModel();

		// Act
		model.CategoryName = "Bug";

		// Assert
		model.CategoryName.Should().Be("Bug");
	}

	[Fact]
	public void CategoryDescription_CanBeSet()
	{
		// Arrange
		var model = new CategoryEditModel();

		// Act
		model.CategoryDescription = "A defect in the software";

		// Assert
		model.CategoryDescription.Should().Be("A defect in the software");
	}

	[Fact]
	public void CategoryDescription_CanBeSetToNull()
	{
		// Arrange
		var model = new CategoryEditModel { CategoryDescription = "Some description" };

		// Act
		model.CategoryDescription = null;

		// Assert
		model.CategoryDescription.Should().BeNull();
	}
}
