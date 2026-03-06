// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentMapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =======================================================

namespace Shared.Mappers;

/// <summary>
/// Unit tests for CommentMapper extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommentMapperTests
{
	[Fact]
	public void ToDto_ShouldMapAllFields_FromCommentModel()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "John Doe", "john@example.com");
		var issue = IssueDto.Empty;
		var comment = new Comment
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Comment Title",
			Description = "Comment description text",
			DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			Issue = issue,
			Author = author,
			DateModified = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = comment.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(comment.Id);
		dto.Title.Should().Be(comment.Title);
		dto.Description.Should().Be(comment.Description);
		dto.DateCreated.Should().Be(comment.DateCreated);
		dto.Issue.Should().Be(comment.Issue);
		dto.Author.Should().Be(comment.Author);
		dto.DateModified.Should().Be(comment.DateModified);
	}

	[Fact]
	public void ToDto_ShouldHandleNullDateModified()
	{
		// Arrange
		var comment = new Comment
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Test Comment",
			Description = "Test description",
			DateCreated = DateTime.UtcNow,
			Issue = IssueDto.Empty,
			Author = UserDto.Empty,
			DateModified = null
		};

		// Act
		var dto = comment.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToDto_ShouldConvertObjectIdToString()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var comment = new Comment
		{
			Id = commentId,
			Title = "Another Comment",
			Description = "Another description",
			DateCreated = DateTime.UtcNow,
			Issue = IssueDto.Empty,
			Author = UserDto.Empty
		};

		// Act
		var dto = comment.ToDto();

		// Assert
		dto.Id.Should().Be(commentId);
	}

	[Fact]
	public void ToModel_ShouldMapAllFields_FromCommentDto()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Jane Smith", "jane@example.com");
		var issue = IssueDto.Empty;
		var dto = new CommentDto(
			ObjectId.GenerateNewId(),
			"DTO Comment",
			"DTO description",
			new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
			issue,
			author,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.Title.Should().Be(dto.Title);
		model.Description.Should().Be(dto.Description);
		model.DateCreated.Should().Be(dto.DateCreated);
		model.Issue.Should().Be(dto.Issue);
		model.Author.Should().Be(dto.Author);
		model.DateModified.Should().Be(dto.DateModified);
	}

	[Fact]
	public void ToModel_ShouldHandleNullDateModified()
	{
		// Arrange
		var dto = new CommentDto(
			ObjectId.GenerateNewId(),
			"Test Title",
			"Test Description",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToDto_ToModel_ShouldRoundTrip()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Alice Brown", "alice@example.com");
		var issue = IssueDto.Empty;
		var original = new Comment
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Original Comment",
			Description = "Original description",
			DateCreated = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
			Issue = issue,
			Author = author,
			DateModified = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = original.ToDto();
		var roundTripped = dto.ToModel();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Title.Should().Be(original.Title);
		roundTripped.Description.Should().Be(original.Description);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.Issue.Should().Be(original.Issue);
		roundTripped.Author.Should().Be(original.Author);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}

	[Fact]
	public void ToModel_ToDto_ShouldRoundTrip()
	{
		// Arrange
		var author = new UserDto("507f1f77bcf86cd799439011", "Bob Wilson", "bob@example.com");
		var issue = IssueDto.Empty;
		var commentId = ObjectId.GenerateNewId();
		var original = new CommentDto(
			commentId,
			"Original DTO",
			"Original DTO description",
			new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc),
			issue,
			author,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		// Act
		var model = original.ToModel();
		var roundTripped = model.ToDto();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.Title.Should().Be(original.Title);
		roundTripped.Description.Should().Be(original.Description);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.Issue.Should().Be(original.Issue);
		roundTripped.Author.Should().Be(original.Author);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}
}
