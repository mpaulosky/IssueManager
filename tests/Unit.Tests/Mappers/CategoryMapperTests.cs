// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryMapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using MongoDB.Bson;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;

namespace Tests.Unit.Mappers;

/// <summary>
/// Unit tests for CategoryMapper extension methods.
/// </summary>
public class CategoryMapperTests
{
	[Fact]
	public void ToDto_ShouldMapAllFields_FromCategoryModel()
	{
		// Arrange
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Bug",
			CategoryDescription = "Software bugs",
			DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			DateModified = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = category.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(category.Id);
		dto.CategoryName.Should().Be(category.CategoryName);
		dto.CategoryDescription.Should().Be(category.CategoryDescription);
		dto.DateCreated.Should().Be(category.DateCreated);
		dto.DateModified.Should().Be(category.DateModified);
	}

	[Fact]
	public void ToDto_ShouldHandleNullDateModified()
	{
		// Arrange
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Feature",
			CategoryDescription = "New features",
			DateCreated = DateTime.UtcNow,
			DateModified = null
		};

		// Act
		var dto = category.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToModel_ShouldMapAllFields_FromCategoryDto()
	{
		// Arrange
		var dto = new CategoryDto(
			ObjectId.GenerateNewId(),
			"Enhancement",
			"Improvements to existing features",
			new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
			false,
			UserDto.Empty);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Id.Should().Be(dto.Id);
		model.CategoryName.Should().Be(dto.CategoryName);
		model.CategoryDescription.Should().Be(dto.CategoryDescription);
		model.DateCreated.Should().Be(dto.DateCreated);
		model.DateModified.Should().Be(dto.DateModified);
	}

	[Fact]
	public void ToModel_ShouldHandleNullDateModified()
	{
		// Arrange
		var dto = new CategoryDto(
			ObjectId.GenerateNewId(),
			"Documentation",
			"Documentation updates",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToDto_ToModel_ShouldRoundTrip()
	{
		// Arrange
		var original = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Performance",
			CategoryDescription = "Performance improvements",
			DateCreated = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
			DateModified = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = original.ToDto();
		var roundTripped = dto.ToModel();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.CategoryName.Should().Be(original.CategoryName);
		roundTripped.CategoryDescription.Should().Be(original.CategoryDescription);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}

	[Fact]
	public void ToModel_ToDto_ShouldRoundTrip()
	{
		// Arrange
		var original = new CategoryDto(
			ObjectId.GenerateNewId(),
			"Security",
			"Security vulnerabilities",
			new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc),
			false,
			UserDto.Empty);

		// Act
		var model = original.ToModel();
		var roundTripped = model.ToDto();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.CategoryName.Should().Be(original.CategoryName);
		roundTripped.CategoryDescription.Should().Be(original.CategoryDescription);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}
}
