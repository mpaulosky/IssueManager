// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueRequestTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Unit
// =============================================

using System.ComponentModel.DataAnnotations;

namespace Web.Components;

/// <summary>
/// Unit tests for the <see cref="CreateIssueRequest"/> request model.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateIssueRequestTests
{
	[Fact]
	public void Constructor_SetsDefaultValues()
	{
		// Act
		var request = new CreateIssueRequest();

		// Assert
		request.Title.Should().Be(string.Empty);
		request.Description.Should().BeNull();
		request.Status.Should().Be("Open");
		request.CategoryId.Should().BeNull();
		request.StatusId.Should().BeNull();
	}

	[Fact]
	public void Title_WithValidValue_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Valid Title" };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Title_WithEmptyValue_FailsValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = string.Empty };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeFalse();
		results.Should().ContainSingle();
		results[0].ErrorMessage.Should().Be("Title is required.");
		results[0].MemberNames.Should().Contain("Title");
	}

	[Fact]
	public void Title_WithTooShortValue_FailsValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "AB" };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeFalse();
		results.Should().ContainSingle();
		results[0].ErrorMessage.Should().Be("Title must be between 3 and 200 characters.");
		results[0].MemberNames.Should().Contain("Title");
	}

	[Fact]
	public void Title_WithTooLongValue_FailsValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = new string('A', 201) };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeFalse();
		results.Should().ContainSingle();
		results[0].ErrorMessage.Should().Be("Title must be between 3 and 200 characters.");
		results[0].MemberNames.Should().Contain("Title");
	}

	[Fact]
	public void Title_WithMaxLength_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = new string('A', 200) };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Title_WithMinLength_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "ABC" };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Description_WithNullValue_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Valid Title", Description = null };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Description_WithValidValue_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Valid Title", Description = "Valid description" };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Description_WithTooLongValue_FailsValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Valid Title", Description = new string('A', 5001) };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeFalse();
		results.Should().ContainSingle();
		results[0].ErrorMessage.Should().Be("Description cannot exceed 5000 characters.");
		results[0].MemberNames.Should().Contain("Description");
	}

	[Fact]
	public void Description_WithMaxLength_PassesValidation()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Valid Title", Description = new string('A', 5000) };
		var context = new ValidationContext(request);
		var results = new List<ValidationResult>();

		// Act
		var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

		// Assert
		isValid.Should().BeTrue();
		results.Should().BeEmpty();
	}

	[Fact]
	public void Status_CanBeSet()
	{
		// Arrange
		var request = new CreateIssueRequest { Title = "Test", Status = "Closed" };

		// Act & Assert
		request.Status.Should().Be("Closed");
	}

	[Fact]
	public void CategoryId_CanBeSet()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId().ToString();
		var request = new CreateIssueRequest { Title = "Test", CategoryId = categoryId };

		// Act & Assert
		request.CategoryId.Should().Be(categoryId);
	}

	[Fact]
	public void StatusId_CanBeSet()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId().ToString();
		var request = new CreateIssueRequest { Title = "Test", StatusId = statusId };

		// Act & Assert
		request.StatusId.Should().Be(statusId);
	}
}
