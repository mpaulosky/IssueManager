using FluentAssertions;

using MongoDB.Bson;

using Shared.DTOs;
using Shared.Validators;

namespace Tests.Unit.Validators;

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
			Status = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Theory]
	[InlineData("Open", "Issue is open")]
	[InlineData("InProgress", "Issue is in progress")]
	[InlineData("Closed", "Issue is closed")]
	public void UpdateIssueStatusValidator_AllValidStatuses_ReturnsNoErrors(string statusName, string statusDesc)
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "issue-456",
			Status = new StatusDto(ObjectId.GenerateNewId(), statusName, statusDesc, DateTime.UtcNow, null, false, UserDto.Empty)
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
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
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
	public void UpdateIssueStatusValidator_EmptyStatusName_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "issue-789",
			Status = new StatusDto(ObjectId.GenerateNewId(), "", "some description", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Status.StatusName");
		result.Errors[0].ErrorMessage.Should().Contain("Status name is required");
	}
}
