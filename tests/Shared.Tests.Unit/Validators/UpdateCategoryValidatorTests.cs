// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =======================================================
namespace Unit.Validators;

/// <summary>
/// Unit tests for <see cref="UpdateCategoryValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateCategoryValidatorTests
{
	private readonly UpdateCategoryValidator _validator = new();

	[Fact]
	public void UpdateCategoryValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Enhancement",
			CategoryDescription = "Improvements to existing features"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void UpdateCategoryValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.Empty,
			CategoryName = "Bug"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateCategoryValidator_EmptyCategoryName_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = ""
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateCategoryValidator_CategoryNameTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "A"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("at least 2 characters"));
	}

	[Fact]
	public void UpdateCategoryValidator_CategoryNameTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = new string('A', 101)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("cannot exceed 100 characters"));
	}

	[Fact]
	public void UpdateCategoryValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Bug",
			CategoryDescription = new string('X', 501)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryDescription" && e.ErrorMessage.Contains("cannot exceed 500 characters"));
	}
}
