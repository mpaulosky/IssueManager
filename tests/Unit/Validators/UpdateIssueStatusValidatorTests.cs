using FluentAssertions;
using IssueManager.Shared.Domain;
using IssueManager.Shared.Validators;

namespace IssueManager.Tests.Unit.Validators;

/// <summary>
/// Unit tests for <see cref="UpdateIssueStatusValidator"/>.
/// </summary>
public class UpdateIssueStatusValidatorTests
{
	private readonly UpdateIssueStatusValidator _validator = new();

	[Fact]
	public void UpdateIssueStatusValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "issue-123",
			Status = IssueStatus.InProgress
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Theory]
	[InlineData(IssueStatus.Open)]
	[InlineData(IssueStatus.InProgress)]
	[InlineData(IssueStatus.Closed)]
	public void UpdateIssueStatusValidator_AllValidStatuses_ReturnsNoErrors(IssueStatus status)
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "issue-456",
			Status = status
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void UpdateIssueStatusValidator_EmptyIssueId_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "",
			Status = IssueStatus.Closed
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("IssueId");
		result.Errors[0].ErrorMessage.Should().Contain("required");
	}

	[Fact]
	public void UpdateIssueStatusValidator_InvalidEnumValue_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "issue-789",
			Status = (IssueStatus)999
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Status");
		result.Errors[0].ErrorMessage.Should().Contain("valid IssueStatus value");
	}
}
