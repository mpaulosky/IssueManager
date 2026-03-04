// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests
// =======================================================
namespace Unit.Validators;

/// <summary>
/// Unit tests for <see cref="UpdateCommentValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateCommentValidatorTests
{
	private readonly UpdateCommentValidator _validator = new();

	[Fact]
	public void UpdateCommentValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Updated Title",
			CommentText = "Updated comment text"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void UpdateCommentValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = "",
			Title = "Title",
			CommentText = "Comment text"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateCommentValidator_EmptyCommentText_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Title",
			CommentText = ""
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CommentText" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void UpdateCommentValidator_CommentTextTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Title",
			CommentText = new string('X', 5001)
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CommentText" && e.ErrorMessage.Contains("cannot exceed 5000 characters"));
	}
}
