using FluentAssertions;
using Shared.Validators;

namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="CreateStatusValidator"/>.
/// </summary>
public class CreateStatusValidatorTests
{
	private readonly CreateStatusValidator _validator = new();

	[Fact]
	public void CreateStatusValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Open",
			StatusDescription = "Issue is open for work"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateStatusValidator_ValidCommandWithoutDescription_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Closed"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateStatusValidator_EmptyStatusName_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateStatusCommand { StatusName = "" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
		result.Errors.Should().Contain(e => e.PropertyName == "StatusName" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void CreateStatusValidator_StatusNameTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateStatusCommand { StatusName = "A" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("StatusName");
		result.Errors[0].ErrorMessage.Should().Contain("at least 2 characters");
	}

	[Fact]
	public void CreateStatusValidator_StatusNameExactly2Characters_IsValid()
	{
		// Arrange
		var command = new CreateStatusCommand { StatusName = "OK" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateStatusValidator_StatusNameExactly100Characters_IsValid()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = new string('A', 100)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateStatusValidator_StatusNameTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = new string('A', 101)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("StatusName");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 100 characters");
	}

	[Fact]
	public void CreateStatusValidator_DescriptionExactly500Characters_IsValid()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Open",
			StatusDescription = new string('X', 500)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateStatusValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Open",
			StatusDescription = new string('X', 501)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("StatusDescription");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 500 characters");
	}

	[Fact]
	public void CreateStatusValidator_NullDescription_IsValid()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Open",
			StatusDescription = null
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
