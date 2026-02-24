// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueMapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using MongoDB.Bson;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;

namespace Tests.Unit.Mappers;

/// <summary>
/// Unit tests for IssueMapper extension methods.
/// </summary>
public class IssueMapperTests
{
	[Fact]
	public void ToDto_ShouldMapAllFields_FromIssueModel()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "John Doe", "john@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Bug", "Software bugs");
		var status = new StatusDto("Open", "Issue is open");
		var archivedBy = new UserDto("507f1f77bcf86cd799439012", "Jane Smith", "jane@example.com");
		var issue = new Issue
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Issue",
			Description = "Test description",
			DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			Author = author,
			Category = category,
			IssueStatus = status,
			Archived = true,
			ArchivedBy = archivedBy,
			DateModified = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = issue.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(issue.Id);
		dto.Title.Should().Be(issue.Title);
		dto.Description.Should().Be(issue.Description);
		dto.DateCreated.Should().Be(issue.DateCreated);
		dto.Author.Should().Be(issue.Author);
		dto.Category.Should().Be(issue.Category);
		dto.Status.Should().Be(issue.IssueStatus);
		dto.Archived.Should().Be(issue.Archived);
		dto.ArchivedBy.Should().Be(issue.ArchivedBy);
		dto.DateModified.Should().Be(issue.DateModified);
	}

	[Fact]
	public void ToDto_ShouldHandleNullDateModified()
	{
		// Arrange
		var issue = new Issue
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Issue",
			Description = "Test description",
			DateCreated = DateTime.UtcNow,
			Author = UserDto.Empty,
			Category = CategoryDto.Empty,
			IssueStatus = new StatusDto("Open", "Open"),
			Archived = false,
			ArchivedBy = UserDto.Empty,
			DateModified = null
		};

		// Act
		var dto = issue.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToDto_ShouldMapArchivedStatus_WhenTrue()
	{
		// Arrange
		var issue = new Issue
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Archived Issue",
			Description = "This issue is archived",
			DateCreated = DateTime.UtcNow,
			Author = UserDto.Empty,
			Category = CategoryDto.Empty,
			IssueStatus = new StatusDto("Closed", "Closed"),
			Archived = true,
			ArchivedBy = new UserDto("507f1f77bcf86cd799439011", "Admin", "admin@example.com")
		};

		// Act
		var dto = issue.ToDto();

		// Assert
		dto.Archived.Should().BeTrue();
		dto.ArchivedBy.Should().NotBeNull();
		dto.ArchivedBy?.Name.Should().Be("Admin");
	}

	[Fact]
	public void ToModel_ShouldMapAllFields_FromIssueDto()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Alice Brown", "alice@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Feature", "New features");
		var status = new StatusDto("In Progress", "Work in progress");
		var archivedBy = new UserDto("507f1f77bcf86cd799439012", "Bob Wilson", "bob@example.com");
		var dto = new IssueDto(
			ObjectId.GenerateNewId(),
			"DTO Issue",
			"DTO description",
			new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
			author,
			category,
			status,
			true,
			archivedBy,
			new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc));

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Id.Should().Be(dto.Id);
		model.Title.Should().Be(dto.Title);
		model.Description.Should().Be(dto.Description);
		model.DateCreated.Should().Be(dto.DateCreated);
		model.Author.Should().Be(dto.Author);
		model.Category.Should().Be(dto.Category);
		model.IssueStatus.Should().Be(dto.Status);
		model.Archived.Should().Be(dto.Archived);
		model.ArchivedBy.Should().Be(dto.ArchivedBy);
		model.DateModified.Should().Be(dto.DateModified);
	}

	[Fact]
	public void ToModel_ShouldHandleNullDateModified()
	{
		// Arrange
		var dto = new IssueDto(
			ObjectId.GenerateNewId(),
			"Test Title",
			"Test Description",
			DateTime.UtcNow,
			UserDto.Empty,
			CategoryDto.Empty,
			new StatusDto("Open", "Open"),
			false);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToModel_ShouldHandleNullArchivedBy_WithUserDtoEmpty()
	{
		// Arrange
		var dto = new IssueDto(
			ObjectId.GenerateNewId(),
			"Unarchived Issue",
			"This issue is not archived",
			DateTime.UtcNow,
			UserDto.Empty,
			CategoryDto.Empty,
			new StatusDto("Open", "Open"),
			false);

		// Act
		var model = dto.ToModel();

		// Assert
		model.ArchivedBy.Should().Be(UserDto.Empty);
	}

	[Fact]
	public void ToDto_ToModel_ShouldRoundTrip()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Charlie Davis", "charlie@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Enhancement", "Improvements");
		var status = new StatusDto("Done", "Completed");
		var archivedBy = new UserDto("507f1f77bcf86cd799439012", "Diana Evans", "diana@example.com");
		var original = new Issue
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original Issue",
			Description = "Original description",
			DateCreated = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
			Author = author,
			Category = category,
			IssueStatus = status,
			Archived = true,
			ArchivedBy = archivedBy,
			DateModified = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = original.ToDto();
		var roundTripped = dto.ToModel();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.Title.Should().Be(original.Title);
		roundTripped.Description.Should().Be(original.Description);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.Author.Should().Be(original.Author);
		roundTripped.Category.Should().Be(original.Category);
		roundTripped.IssueStatus.Should().Be(original.IssueStatus);
		roundTripped.Archived.Should().Be(original.Archived);
		roundTripped.ArchivedBy.Should().Be(original.ArchivedBy);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}

	[Fact]
	public void ToModel_ToDto_ShouldRoundTrip()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Eve Foster", "eve@example.com");
		var category = new CategoryDto(ObjectId.GenerateNewId(), "Security", "Security issues");
		var status = new StatusDto("Closed", "Resolved");
		var archivedBy = new UserDto("507f1f77bcf86cd799439012", "Frank Green", "frank@example.com");
		var original = new IssueDto(
			ObjectId.GenerateNewId(),
			"Original DTO",
			"Original DTO description",
			new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
			author,
			category,
			status,
			true,
			archivedBy,
			new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc));

		// Act
		var model = original.ToModel();
		var roundTripped = model.ToDto();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.Title.Should().Be(original.Title);
		roundTripped.Description.Should().Be(original.Description);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.Author.Should().Be(original.Author);
		roundTripped.Category.Should().Be(original.Category);
		roundTripped.Status.Should().Be(original.Status);
		roundTripped.Archived.Should().Be(original.Archived);
		roundTripped.ArchivedBy.Should().Be(original.ArchivedBy);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}
}
