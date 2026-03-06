// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     HelpersTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================
// File Name :     HelpersTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit

namespace Shared.Helpers;

/// <summary>
/// Unit tests for <see cref="Helpers"/>.
/// </summary>
public sealed class HelpersTests
{
	[Fact]
	public void GetStaticDate_ReturnsExpectedDate()
	{
		// Arrange
		var expectedDate = new DateTimeOffset(2025, 1, 1, 8, 0, 0, TimeSpan.Zero);

		// Act
		var result = Shared.Helpers.Helpers.GetStaticDate();

		// Assert
		result.Should().Be(expectedDate);
	}

	[Fact]
	public void GetStaticDate_ReturnsSameValueOnRepeatedCalls()
	{
		// Arrange / Act
		var result1 = Shared.Helpers.Helpers.GetStaticDate();
		var result2 = Shared.Helpers.Helpers.GetStaticDate();

		// Assert
		result1.Should().Be(result2);
	}

	[Fact]
	public void GenerateSlug_EmptyString_ReturnsEmpty()
	{
		// Arrange
		var input = "";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void GenerateSlug_NullOrWhitespace_ReturnsEmpty()
	{
		// Arrange / Act
		var resultNull = ((string)null!).GenerateSlug();
		var resultWhitespace = "   ".GenerateSlug();

		// Assert
		resultNull.Should().BeEmpty();
		resultWhitespace.Should().BeEmpty();
	}

	[Fact]
	public void GenerateSlug_HelloWorld_ReturnsHelloUnderscore()
	{
		// Arrange
		var input = "Hello World";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("hello_world");
	}

	[Fact]
	public void GenerateSlug_CSharpIsGreat_ReturnsCUnderscoreIsUnderscoreGreat()
	{
		// Arrange
		var input = "C# Is Great!";

		// Act
		var result = input.GenerateSlug();

		// Assert
		// "C# Is Great!" ends with '!' (non-alphanumeric) AND has '#' internally,
		// so GenerateSlug appends a trailing underscore per its documented behavior.
		result.Should().Be("c_is_great_");
	}

	[Fact]
	public void GenerateSlug_AlreadySlug_ReturnsUnchanged()
	{
		// Arrange
		var input = "already_slug";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("already_slug");
	}

	[Fact]
	public void GenerateSlug_MultipleSpaces_CollapsesToSingleUnderscore()
	{
		// Arrange
		var input = "multiple   spaces";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("multiple_spaces");
	}

	[Fact]
	public void GenerateSlug_LeadingAndTrailingSpaces_TrimsCorrectly()
	{
		// Arrange
		var input = "  leading and trailing  ";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("leading_and_trailing");
	}

	[Fact]
	public void GenerateSlug_Uppercase_ReturnsLowercase()
	{
		// Arrange
		var input = "UPPERCASE";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("uppercase");
	}

	[Fact]
	public void GenerateSlug_MultipleHyphens_CollapsesToSingleUnderscore()
	{
		// Arrange
		var input = "hello--world";

		// Act
		var result = input.GenerateSlug();

		// Assert
		result.Should().Be("hello_world");
	}

	[Fact]
	public void GetRandomCategoryName_ReturnsValidCategory()
	{
		// Arrange
		var validCategories = new[]
		{
			MyCategories.First, MyCategories.Second, MyCategories.Third,
			MyCategories.Fourth, MyCategories.Fifth, MyCategories.Sixth,
			MyCategories.Seventh, MyCategories.Eighth, MyCategories.Ninth
		};

		// Act
		var result = Shared.Helpers.Helpers.GetRandomCategoryName();

		// Assert
		result.Should().BeOneOf(validCategories);
	}

	[Fact]
	public void GetRandomCategoryName_MultipleCallsReturnValidCategories()
	{
		// Arrange
		var validCategories = new[]
		{
			MyCategories.First, MyCategories.Second, MyCategories.Third,
			MyCategories.Fourth, MyCategories.Fifth, MyCategories.Sixth,
			MyCategories.Seventh, MyCategories.Eighth, MyCategories.Ninth
		};

		// Act
		var results = Enumerable.Range(0, 20).Select(_ => Shared.Helpers.Helpers.GetRandomCategoryName()).ToList();

		// Assert
		results.Should().OnlyContain(r => validCategories.Contains(r));
	}
}
