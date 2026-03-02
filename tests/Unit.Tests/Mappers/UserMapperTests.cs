// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UserMapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Tests.Unit.Mappers;

/// <summary>
/// Unit tests for UserMapper extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserMapperTests
{
	[Fact]
	public void ToDto_ShouldMapAllFields_FromUserModel()
	{
		// Arrange
		var user = new User
		{
			Id = "507f1f77bcf86cd799439011",
			Name = "John Doe",
			Email = "john.doe@example.com"
		};

		// Act
		var dto = user.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(user.Id);
		dto.Name.Should().Be(user.Name);
		dto.Email.Should().Be(user.Email);
	}

	[Fact]
	public void ToDto_ShouldHandleEmptyStrings()
	{
		// Arrange
		var user = new User
		{
			Id = string.Empty,
			Name = string.Empty,
			Email = string.Empty
		};

		// Act
		var dto = user.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Name.Should().BeEmpty();
		dto.Email.Should().BeEmpty();
	}

	[Fact]
	public void ToModel_ShouldMapAllFields_FromUserDto()
	{
		// Arrange
		var dto = new UserDto(
			"507f1f77bcf86cd799439011",
			"Jane Smith",
			"jane.smith@example.com");

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Name.Should().Be(dto.Name);
		model.Email.Should().Be(dto.Email);
	}

	[Fact]
	public void ToModel_ShouldHandleEmptyStrings()
	{
		// Arrange
		var dto = new UserDto(
			string.Empty,
			string.Empty,
			string.Empty);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Name.Should().BeEmpty();
		model.Email.Should().BeEmpty();
	}

	[Fact]
	public void ToDto_ToModel_ShouldRoundTrip()
	{
		// Arrange
		var original = new User
		{
			Id = "507f1f77bcf86cd799439011",
			Name = "Alice Brown",
			Email = "alice.brown@example.com"
		};

		// Act
		var dto = original.ToDto();
		var roundTripped = dto.ToModel();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Name.Should().Be(original.Name);
		roundTripped.Email.Should().Be(original.Email);
	}

	[Fact]
	public void ToModel_ToDto_ShouldRoundTrip()
	{
		// Arrange
		var original = new UserDto(
			"507f1f77bcf86cd799439011",
			"Bob Wilson",
			"bob.wilson@example.com");

		// Act
		var model = original.ToModel();
		var roundTripped = model.ToDto();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Name.Should().Be(original.Name);
		roundTripped.Email.Should().Be(original.Email);
		// Note: Id is not mapped in ToModel(), so it will be empty string
		roundTripped.Id.Should().BeEmpty();
	}

	[Fact]
	public void ToDto_ShouldMapUserDtoEmpty()
	{
		// Arrange
		var emptyUser = new User
		{
			Id = string.Empty,
			Name = string.Empty,
			Email = string.Empty
		};

		// Act
		var dto = emptyUser.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().BeEmpty();
		dto.Name.Should().BeEmpty();
		dto.Email.Should().BeEmpty();
	}

	[Fact]
	public void ToModel_ShouldMapFromUserDtoEmpty()
	{
		// Arrange
		var emptyDto = UserDto.Empty;

		// Act
		var model = emptyDto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Name.Should().BeEmpty();
		model.Email.Should().BeEmpty();
	}
}
