using FluentAssertions;
using IssueManager.Shared.Validators;

namespace IssueManager.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateIssueValidator.
/// </summary>
public class UpdateIssueValidatorTests
{
	private readonly UpdateIssueValidator _validator = new();

	[Fact]
	public void UpdateIssueValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "Updated Bug Fix",
			Description = "Fixed the authentication issue"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void UpdateIssueValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = "",
			Title = "Valid Title",
			Description = "Valid Description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Id");
		result.Errors[0].ErrorMessage.Should().Contain("required");
	}

	[Fact]
	public void UpdateIssueValidator_EmptyTitle_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "",
			Description = "Some description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterOrEqualTo(1);
		result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateIssueValidator_TitleTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "AB",
			Description = "Some description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Title");
		result.Errors[0].ErrorMessage.Should().Contain("at least 3 characters");
	}

	[Fact]
	public void UpdateIssueValidator_TitleExactly3Characters_IsValid()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "Bug",
			Description = "Description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void UpdateIssueValidator_TitleExactly256Characters_IsValid()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = new string('A', 256),
			Description = "Description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void UpdateIssueValidator_TitleTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = new string('A', 257),
			Description = "Some description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Title");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 256 characters");
	}

	[Fact]
	public void UpdateIssueValidator_NullDescription_IsValid()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "Valid Title",
			Description = null
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void UpdateIssueValidator_DescriptionExactly4096Characters_IsValid()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "Valid Title",
			Description = new string('X', 4096)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void UpdateIssueValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
			Title = "Valid Title",
			Description = new string('X', 4097)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Description");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 4096 characters");
	}
}
