// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UserDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================

namespace Shared.DTOs;

/// <summary>
/// Unit tests for <see cref="UserDto"/>.
/// </summary>
public sealed class UserDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = UserDto.Empty;

		// Assert
		dto.Id.Should().BeEmpty();
		dto.Name.Should().BeEmpty();
		dto.Email.Should().BeEmpty();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var id = "user123";
		var name = "John Doe";
		var email = "john@example.com";

		// Act
		var dto = new UserDto(id, name, email);

		// Assert
		dto.Id.Should().Be(id);
		dto.Name.Should().Be(name);
		dto.Email.Should().Be(email);
	}

	[Fact]
	public void ModelConstructor_MapsAllFieldsCorrectly()
	{
		// Arrange
		var user = new User
		{
			Id = "user456",
			Name = "Jane Smith",
			Email = "jane@example.com"
		};

		// Act
		var dto = new UserDto(user);

		// Assert
		dto.Id.Should().Be(user.Id);
		dto.Name.Should().Be(user.Name);
		dto.Email.Should().Be(user.Email);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var id = "user123";
		var name = "John Doe";
		var email = "john@example.com";

		// Act
		var dto1 = new UserDto(id, name, email);
		var dto2 = new UserDto(id, name, email);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var id = "user123";
		var name = "John Doe";
		var email = "john@example.com";

		// Act
		var dto1 = new UserDto(id, name, email);
		var dto2 = new UserDto(id, name, email);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
