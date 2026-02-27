// Copyright (c) 2026. All rights reserved.
// File Name :     CommentDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

using MongoDB.Bson;
using Shared.DTOs;
using Shared.Models;

namespace Tests.Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="CommentDto"/>.
/// </summary>
public sealed class CommentDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = CommentDto.Empty;

		// Assert
		dto.Id.Should().Be(ObjectId.Empty);
		dto.Title.Should().BeEmpty();
		dto.Description.Should().BeEmpty();
		dto.DateCreated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		// IssueDto.Empty creates a new instance each call (DateCreated = UtcNow), so compare fields not by reference/value equality
		dto.Issue.Id.Should().Be(ObjectId.Empty);
		dto.Issue.Title.Should().BeEmpty();
		dto.Issue.Description.Should().BeEmpty();
		dto.Issue.Author.Should().Be(UserDto.Empty);
		dto.Issue.Archived.Should().BeFalse();
		dto.Issue.DateModified.Should().BeNull();
		dto.Author.Should().Be(UserDto.Empty);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Comment";
		var description = "Comment description";
		var dateCreated = DateTime.UtcNow;
		var issue = new IssueDto(ObjectId.GenerateNewId(), "Issue", "Description", DateTime.UtcNow,
			null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto = new CommentDto(id, title, description, dateCreated, dateModified, issue, author, [], false, UserDto.Empty, false, UserDto.Empty);

		// Assert
		dto.Id.Should().Be(id);
		dto.Title.Should().Be(title);
		dto.Description.Should().Be(description);
		dto.DateCreated.Should().Be(dateCreated);
		dto.Issue.Should().Be(issue);
		dto.Author.Should().Be(author);
		dto.DateModified.Should().Be(dateModified);
	}

	[Fact]
	public void Constructor_DefaultsOptionalParameters()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Comment";
		var description = "Comment description";
		var dateCreated = DateTime.UtcNow;
		var issue = new IssueDto(ObjectId.GenerateNewId(), "Issue", "Description", DateTime.UtcNow,
			null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);
		var author = new UserDto("user1", "John Doe", "john@example.com");

		// Act
		var dto = new CommentDto(id, title, description, dateCreated, null, issue, author, [], false, UserDto.Empty, false, UserDto.Empty);

		// Assert
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ModelConstructor_MapsAllFieldsCorrectly()
	{
		// Arrange
		var issueDto = new IssueDto(ObjectId.GenerateNewId(), "Issue", "Description", DateTime.UtcNow,
			null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);
		var authorDto = new UserDto("user1", "John Doe", "john@example.com");

		var comment = new Comment
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Model Comment",
			Description = "Model comment description",
			DateCreated = DateTime.UtcNow,
			Issue = issueDto,
			Author = authorDto,
			DateModified = DateTime.UtcNow.AddDays(2)
		};

		// Act
		var dto = new CommentDto(comment);

		// Assert
		dto.Id.Should().Be(comment.Id);
		dto.Title.Should().Be(comment.Title);
		dto.Description.Should().Be(comment.Description);
		dto.DateCreated.Should().Be(comment.DateCreated);
		dto.Issue.Should().Be(comment.Issue);
		dto.Author.Should().Be(comment.Author);
		dto.DateModified.Should().Be(comment.DateModified);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Comment";
		var description = "Comment description";
		var dateCreated = DateTime.UtcNow;
		var issue = new IssueDto(ObjectId.GenerateNewId(), "Issue", "Description", DateTime.UtcNow,
			null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);
		var author = new UserDto("user1", "John Doe", "john@example.com");
		var dateModified = DateTime.UtcNow.AddDays(1);
		var userVotes = new HashSet<string>();

		// Act
		var dto1 = new CommentDto(id, title, description, dateCreated, dateModified, issue, author, userVotes, false, UserDto.Empty, false, UserDto.Empty);
		var dto2 = new CommentDto(id, title, description, dateCreated, dateModified, issue, author, userVotes, false, UserDto.Empty, false, UserDto.Empty);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var title = "Test Comment";
		var description = "Comment description";
		var dateCreated = DateTime.UtcNow;
		var issue = new IssueDto(ObjectId.GenerateNewId(), "Issue", "Description", DateTime.UtcNow,
			null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);
		var author = new UserDto("user1", "John Doe", "john@example.com");

		// Act
		var dto1 = new CommentDto(id, title, description, dateCreated, null, issue, author, [], false, UserDto.Empty, false, UserDto.Empty);
		var dto2 = new CommentDto(id, title, description, dateCreated, null, issue, author, [], false, UserDto.Empty, false, UserDto.Empty);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
