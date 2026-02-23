using FluentAssertions;
using MongoDB.Bson;
using Shared.Validators;

namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="UpdateCategoryValidator"/>.
/// </summary>
public class UpdateCategoryValidatorTests
{
	private readonly UpdateCategoryValidator _validator = new();

	[Fact]
	public void UpdateCategoryValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
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
			Id = "",
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
			Id = ObjectId.GenerateNewId().ToString(),
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
			Id = ObjectId.GenerateNewId().ToString(),
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
			Id = ObjectId.GenerateNewId().ToString(),
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
			Id = ObjectId.GenerateNewId().ToString(),
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
