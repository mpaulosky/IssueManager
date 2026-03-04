// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests
// =======================================================
namespace Unit.Validators;

/// <summary>
/// Unit tests for <see cref="UpdateStatusValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateStatusValidatorTests
{
	private readonly UpdateStatusValidator _validator = new();

	[Fact]
	public void UpdateStatusValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = "In Progress",
			StatusDescription = "Currently being worked on"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void UpdateStatusValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = "",
			StatusName = "Open"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateStatusValidator_EmptyStatusName_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = ""
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "StatusName" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateStatusValidator_StatusNameTooShort_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = "A"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "StatusName" && e.ErrorMessage.Contains("at least 2 characters"));
	}

	[Fact]
	public void UpdateStatusValidator_StatusNameTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = new string('A', 101)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "StatusName" && e.ErrorMessage.Contains("cannot exceed 100 characters"));
	}

	[Fact]
	public void UpdateStatusValidator_DescriptionTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = "Open",
			StatusDescription = new string('X', 501)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "StatusDescription" && e.ErrorMessage.Contains("cannot exceed 500 characters"));
	}
}
