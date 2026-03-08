// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =======================================================
namespace Shared.Contracts;

/// <summary>
/// Unit tests for <see cref="CreateCommentValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCommentValidatorTests
{
	private readonly CreateCommentValidator _validator = new();

	[Fact]
	public void CreateCommentValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Comment Title",
			CommentText = "This is a valid comment",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void CreateCommentValidator_EmptyCommentText_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = "",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
		result.Errors.Should().Contain(e => e.PropertyName == "CommentText" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void CreateCommentValidator_CommentTextExactly1Character_IsValid()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = "A",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateCommentValidator_CommentTextExactly5000Characters_IsValid()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = new string('X', 5000),
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void CreateCommentValidator_CommentTextTooLong_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = new string('X', 5001),
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("CommentText");
		result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 5000 characters");
	}

	[Fact]
	public void CreateCommentValidator_EmptyIssueId_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = "Comment text",
			IssueId = ""
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "IssueId" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void CreateCommentValidator_InvalidIssueId_ReturnsValidationError()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = "Comment text",
			IssueId = "not-a-valid-objectid"
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "IssueId" && e.ErrorMessage.Contains("valid ObjectId"));
	}

	[Fact]
	public void CreateCommentValidator_ValidObjectId_PassesValidation()
	{
		// Arrange
		var validObjectId = ObjectId.GenerateNewId().ToString();
		var command = new CreateCommentCommand
		{
			Title = "Title",
			CommentText = "Comment text",
			IssueId = validObjectId
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
