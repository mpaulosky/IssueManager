using FluentAssertions;

using Shared.Validators;

namespace IssueManager.Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="CreateIssueValidator"/>.
/// </summary>
public class CreateIssueValidatorTests
{
	private readonly CreateIssueValidator _validator = new();

	[Fact]
	public void CreateIssueValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Fix login bug",
			Description = "SSO authentication is broken for external users"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateIssueValidator_ValidCommandWithLabels_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Add dark mode support",
			Description = "Users want dark mode",
			Labels = new List<string> { "feature", "ui" }
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateIssueValidator_EmptyTitle_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand { Title = "", Description = "Some description" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterOrEqualTo(1);
		result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void CreateIssueValidator_TitleTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand { Title = "Hi", Description = "Some description" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Title");
		result.Errors[0].ErrorMessage.Should().Contain("at least 3 characters");
	}

	[Fact]
	public void CreateIssueValidator_TitleExactly3Characters_IsValid()
	{
		// Arrange
		var command = new CreateIssueCommand { Title = "Bug", Description = "Description" };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateIssueValidator_TitleExactly200Characters_IsValid()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = new string('A', 200),
			Description = "Description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateIssueValidator_TitleTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = new string('A', 201),
			Description = "Some description"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Title");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 200 characters");
	}

	[Fact]
	public void CreateIssueValidator_NullDescription_IsValid()
	{
		// Arrange
		var command = new CreateIssueCommand { Title = "Valid Title", Description = null };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateIssueValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = new string('X', 5001)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Description");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 5000 characters");
	}

	[Fact]
	public void CreateIssueValidator_EmptyLabelInList_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = "Valid description",
			Labels = new List<string> { "bug", "", "feature" }
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Contain("Labels");
		result.Errors[0].ErrorMessage.Should().Contain("cannot be empty");
	}

	[Fact]
	public void CreateIssueValidator_LabelTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = "Valid description",
			Labels = new List<string> { new string('L', 51) }
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Contain("Labels");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 50 characters");
	}
}
