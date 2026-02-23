using FluentAssertions;
using Shared.Validators;

namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="CreateCategoryValidator"/>.
/// </summary>
public class CreateCategoryValidatorTests
{
	private readonly CreateCategoryValidator _validator = new();

	[Fact]
	public void CreateCategoryValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = "Software bugs and issues"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateCategoryValidator_ValidCommandWithoutDescription_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Feature"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateCategoryValidator_EmptyCategoryName_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand { CategoryName = "" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterOrEqualTo(1);
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void CreateCategoryValidator_CategoryNameTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand { CategoryName = "A" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("CategoryName");
		result.Errors[0].ErrorMessage.Should().Contain("at least 2 characters");
	}

	[Fact]
	public void CreateCategoryValidator_CategoryNameExactly2Characters_IsValid()
	{
		// Arrange
		var command = new CreateCategoryCommand { CategoryName = "UI" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateCategoryValidator_CategoryNameExactly100Characters_IsValid()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = new string('A', 100)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateCategoryValidator_CategoryNameTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = new string('A', 101)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("CategoryName");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 100 characters");
	}

	[Fact]
	public void CreateCategoryValidator_DescriptionExactly500Characters_IsValid()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = new string('X', 500)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateCategoryValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = new string('X', 501)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("CategoryDescription");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 500 characters");
	}

	[Fact]
	public void CreateCategoryValidator_NullDescription_IsValid()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = null
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
