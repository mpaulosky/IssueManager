using FluentAssertions;
using IssueManager.Shared.Validators;

namespace IssueManager.Tests.Unit.Validators;

/// <summary>
/// Unit tests for DeleteIssueValidator (for soft-delete/archive operation).
/// </summary>
public class DeleteIssueValidatorTests
{
	private readonly DeleteIssueValidator _validator = new();

	[Fact]
	public void DeleteIssueValidator_ValidId_ReturnsNoErrors()
	{
		// Arrange
		var command = new DeleteIssueCommand
		{
			Id = Guid.NewGuid().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void DeleteIssueValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteIssueCommand { Id = "" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Id");
		result.Errors[0].ErrorMessage.Should().Contain("required");
	}

	[Fact]
	public void DeleteIssueValidator_NullId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteIssueCommand { Id = null! };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id");
	}

	[Fact]
	public void DeleteIssueValidator_WhitespaceId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteIssueCommand { Id = "   " };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Id");
	}
}
