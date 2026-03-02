// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

namespace Tests.Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="CategoryDto"/>.
/// </summary>
public sealed class CategoryDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = CategoryDto.Empty;

		// Assert
		dto.Id.Should().Be(ObjectId.Empty);
		dto.CategoryName.Should().BeEmpty();
		dto.CategoryDescription.Should().BeEmpty();
		dto.DateCreated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var categoryName = "Bug";
		var categoryDescription = "Bug reports";
		var dateCreated = DateTime.UtcNow;
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto = new CategoryDto(id, categoryName, categoryDescription, dateCreated, dateModified, false, UserDto.Empty);

		// Assert
		dto.Id.Should().Be(id);
		dto.CategoryName.Should().Be(categoryName);
		dto.CategoryDescription.Should().Be(categoryDescription);
		dto.DateCreated.Should().Be(dateCreated);
		dto.DateModified.Should().Be(dateModified);
	}

	[Fact]
	public void Constructor_DefaultsOptionalParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var categoryName = "Bug";
		var categoryDescription = "Bug reports";

		// Act
		var dto = new CategoryDto(id, categoryName, categoryDescription, default, null, false, UserDto.Empty);

		// Assert
		dto.DateCreated.Should().Be(default);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ModelConstructor_MapsAllFieldsCorrectly()
	{
		// Arrange
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Feature",
			CategoryDescription = "New features",
			DateCreated = DateTime.UtcNow,
			DateModified = DateTime.UtcNow.AddDays(2)
		};

		// Act
		var dto = new CategoryDto(category);

		// Assert
		dto.Id.Should().Be(category.Id);
		dto.CategoryName.Should().Be(category.CategoryName);
		dto.CategoryDescription.Should().Be(category.CategoryDescription);
		dto.DateCreated.Should().Be(category.DateCreated);
		dto.DateModified.Should().Be(category.DateModified);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var categoryName = "Bug";
		var categoryDescription = "Bug reports";
		var dateCreated = DateTime.UtcNow;
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto1 = new CategoryDto(id, categoryName, categoryDescription, dateCreated, dateModified, false, UserDto.Empty);
		var dto2 = new CategoryDto(id, categoryName, categoryDescription, dateCreated, dateModified, false, UserDto.Empty);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var categoryName = "Bug";
		var categoryDescription = "Bug reports";

		// Act
		var dto1 = new CategoryDto(id, categoryName, categoryDescription, default, null, false, UserDto.Empty);
		var dto2 = new CategoryDto(id, categoryName, categoryDescription, default, null, false, UserDto.Empty);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
