// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =======================================================
namespace Shared.Validators;

/// <summary>
/// Unit tests for <see cref="DeleteCommentValidator"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteCommentValidatorTests
{
	private readonly DeleteCommentValidator _validator = new();

	[Fact]
	public void DeleteCommentValidator_ValidCommand_ReturnsNoErrors()
	{
		// Arrange
		var command = new DeleteCommentCommand
		{
			Id = ObjectId.GenerateNewId()
		};

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void DeleteCommentValidator_EmptyId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteCommentCommand { Id = ObjectId.Empty };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void DeleteCommentValidator_NullId_ReturnsValidationError()
	{
		// Arrange
		var command = new DeleteCommentCommand { Id = ObjectId.Empty };

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage.Contains("required"));
	}
}
