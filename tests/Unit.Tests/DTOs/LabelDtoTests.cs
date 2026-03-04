// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     LabelDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests
// =============================================
// File Name :     LabelDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

namespace Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="LabelDto"/>.
/// </summary>
public sealed class LabelDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = LabelDto.Empty;

		// Assert
		dto.Name.Should().BeEmpty();
		dto.Color.Should().BeEmpty();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var name = "Bug";
		var color = "#FF0000";

		// Act
		var dto = new LabelDto(name, color);

		// Assert
		dto.Name.Should().Be(name);
		dto.Color.Should().Be(color);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var name = "Bug";
		var color = "#FF0000";

		// Act
		var dto1 = new LabelDto(name, color);
		var dto2 = new LabelDto(name, color);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var name = "Bug";
		var color = "#FF0000";

		// Act
		var dto1 = new LabelDto(name, color);
		var dto2 = new LabelDto(name, color);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
