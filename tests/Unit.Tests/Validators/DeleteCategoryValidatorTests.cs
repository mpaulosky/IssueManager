namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="DeleteCategoryValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteCategoryValidatorTests
{
	private readonly DeleteCategoryValidator _validator = new();

	[Fact]
	public void DeleteCategoryValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new DeleteCategoryCommand
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
	public void DeleteCategoryValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteCategoryCommand { Id = "" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void DeleteCategoryValidator_NullId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteCategoryCommand { Id = null! };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}
}
