namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="DeleteStatusValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteStatusValidatorTests
{
	private readonly DeleteStatusValidator _validator = new();

	[Fact]
	public void DeleteStatusValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new DeleteStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void DeleteStatusValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteStatusCommand { Id = "" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void DeleteStatusValidator_NullId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteStatusCommand { Id = null! };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}
}
