// Copyright (c) 2026. All rights reserved.
// File Name :     IssueDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

namespace Tests.Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="IssueDto"/>.
/// </summary>
public sealed class IssueDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = IssueDto.Empty;

		// Assert
		dto.Id.Should().Be(ObjectId.Empty);
		dto.Title.Should().BeEmpty();
		dto.Description.Should().BeEmpty();
		dto.DateCreated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		dto.Author.Should().Be(UserDto.Empty);
		dto.Category.Id.Should().Be(ObjectId.Empty);
		dto.Category.CategoryName.Should().BeEmpty();
		dto.Status.Id.Should().Be(ObjectId.Empty);
		dto.Status.StatusName.Should().BeEmpty();
		dto.Archived.Should().BeFalse();
		dto.ArchivedBy.Should().Be(UserDto.Empty);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Issue";
		var description = "Test Description";
		var dateCreated = DateTime.UtcNow;
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug reports", default, null, false, UserDto.Empty);
		var status = new StatusDto(ObjectId.Empty, "Open", "Open status", default, null, false, UserDto.Empty);
		var archived = true;
		var archivedBy = new UserDto("user2", "Jane Doe", "jane@example.com");
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto = new IssueDto(id, title, description, dateCreated, dateModified, author, category, status, archived, archivedBy, false, false);

		// Assert
		dto.Id.Should().Be(id);
		dto.Title.Should().Be(title);
		dto.Description.Should().Be(description);
		dto.DateCreated.Should().Be(dateCreated);
		dto.Author.Should().Be(author);
		dto.Category.Should().Be(category);
		dto.Status.Should().Be(status);
		dto.Archived.Should().Be(archived);
		dto.ArchivedBy.Should().Be(archivedBy);
		dto.DateModified.Should().Be(dateModified);
	}

	[Fact]
	public void Constructor_DefaultsOptionalParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Issue";
		var description = "Test Description";
		var dateCreated = DateTime.UtcNow;
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug reports", default, null, false, UserDto.Empty);
		var status = new StatusDto(ObjectId.Empty, "Open", "Open status", default, null, false, UserDto.Empty);

		// Act
		var dto = new IssueDto(id, title, description, dateCreated, null, author, category, status, false, UserDto.Empty, false, false);

		// Assert
		dto.Archived.Should().BeFalse();
		dto.ArchivedBy.Should().Be(UserDto.Empty);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Issue";
		var description = "Test Description";
		var dateCreated = DateTime.UtcNow;
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug reports", default, null, false, UserDto.Empty);
		var status = new StatusDto(ObjectId.Empty, "Open", "Open status", default, null, false, UserDto.Empty);

		// Act
		var dto1 = new IssueDto(id, title, description, dateCreated, null, author, category, status, false, UserDto.Empty, false, false);
		var dto2 = new IssueDto(id, title, description, dateCreated, null, author, category, status, false, UserDto.Empty, false, false);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Issue";
		var description = "Test Description";
		var dateCreated = DateTime.UtcNow;
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Bug reports", default, null, false, UserDto.Empty);
		var status = new StatusDto(ObjectId.Empty, "Open", "Open status", default, null, false, UserDto.Empty);

		// Act
		var dto1 = new IssueDto(id, title, description, dateCreated, null, author, category, status, false, UserDto.Empty, false, false);
		var dto2 = new IssueDto(id, title, description, dateCreated, null, author, category, status, false, UserDto.Empty, false, false);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
