using FluentAssertions;
using Shared.Validators;

namespace Tests.Unit.Validators;

/// <summary>
/// Unit tests for ListIssuesQueryValidator.
/// </summary>
public class ListIssuesQueryValidatorTests
{
	private readonly ListIssuesQueryValidator _validator = new();

	[Fact]
	public void ListIssuesQueryValidator_DefaultValues_ReturnsNoErrors()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 20
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void ListIssuesQueryValidator_MaxPageSize_IsValid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 100
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void ListIssuesQueryValidator_PageZero_ReturnsValidationError()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 0,
			PageSize = 20
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("Page");
		result.Errors[0].ErrorMessage.Should().Contain("greater than or equal to 1");
	}

	[Fact]
	public void ListIssuesQueryValidator_NegativePage_ReturnsValidationError()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = -1,
			PageSize = 20
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Page");
	}

	[Fact]
	public void ListIssuesQueryValidator_PageSizeZero_ReturnsValidationError()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 0
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("PageSize");
		result.Errors[0].ErrorMessage.Should().Contain("between 1 and 100");
	}

	[Fact]
	public void ListIssuesQueryValidator_NegativePageSize_ReturnsValidationError()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = -10
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
	}

	[Fact]
	public void ListIssuesQueryValidator_PageSizeExceedsMax_ReturnsValidationError()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 101
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("PageSize");
		result.Errors[0].ErrorMessage.Should().Contain("100");
	}

	[Fact]
	public void ListIssuesQueryValidator_BothInvalid_ReturnsTwoErrors()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 0,
			PageSize = 101
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(2);
		result.Errors.Should().Contain(e => e.PropertyName == "Page");
		result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
	}

	[Fact]
	public void Validate_WithSearchTerm_ReturnsValid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 10,
			SearchTerm = "bug fix"
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithAuthorName_ReturnsValid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 10,
			AuthorName = "John"
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithBothFilters_ReturnsValid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 10,
			SearchTerm = "bug",
			AuthorName = "Alice"
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithSearchTermTooLong_ReturnsInvalid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 10,
			SearchTerm = new string('a', 201) // 201 characters
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("SearchTerm");
		result.Errors[0].ErrorMessage.Should().Contain("200");
	}

	[Fact]
	public void Validate_WithAuthorNameTooLong_ReturnsInvalid()
	{
		// Arrange
		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 10,
			AuthorName = new string('b', 201) // 201 characters
		};

		// Act
		var result = _validator.Validate(query);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().HaveCount(1);
		result.Errors[0].PropertyName.Should().Be("AuthorName");
		result.Errors[0].ErrorMessage.Should().Contain("200");
	}
}
