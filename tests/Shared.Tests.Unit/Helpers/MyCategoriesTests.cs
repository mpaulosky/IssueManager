// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MyCategoriesTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================

namespace Shared.Helpers;

/// <summary>
/// Unit tests for <see cref="MyCategories"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MyCategoriesTests
{
	[Fact]
	public void First_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.First.Should().Be("Bug");
	}

	[Fact]
	public void Second_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Second.Should().Be("Feature");
	}

	[Fact]
	public void Third_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Third.Should().Be("Enhancement");
	}

	[Fact]
	public void Fourth_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Fourth.Should().Be("Documentation");
	}

	[Fact]
	public void Fifth_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Fifth.Should().Be("Performance");
	}

	[Fact]
	public void Sixth_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Sixth.Should().Be("Security");
	}

	[Fact]
	public void Seventh_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Seventh.Should().Be("Tech Debt");
	}

	[Fact]
	public void Eighth_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Eighth.Should().Be("Question");
	}

	[Fact]
	public void Ninth_HasExpectedValue()
	{
		// Arrange / Act / Assert
		MyCategories.Ninth.Should().Be("Other");
	}

	[Fact]
	public void AllConstants_HaveDistinctValues()
	{
		// Arrange
		var categories = new[]
		{
			MyCategories.First, MyCategories.Second, MyCategories.Third,
			MyCategories.Fourth, MyCategories.Fifth, MyCategories.Sixth,
			MyCategories.Seventh, MyCategories.Eighth, MyCategories.Ninth
		};

		// Act / Assert
		categories.Should().OnlyHaveUniqueItems();
	}
}
