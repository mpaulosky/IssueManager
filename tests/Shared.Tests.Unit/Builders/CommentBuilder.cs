// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentBuilder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =======================================================

namespace Unit.Builders;

/// <summary>
/// Builder for creating CommentDto test data with fluent API.
/// </summary>
[ExcludeFromCodeCoverage]
public class CommentBuilder
{
	private string _id = ObjectId.GenerateNewId().ToString();
	private string _title = "Default Comment Title";
	private string _description = "Default comment description";
	private DateTime _dateCreated = DateTime.UtcNow;
	private IssueDto _issue = IssueDto.Empty;
	private UserDto _author = UserDto.Empty;
	private DateTime? _dateModified;

	public CommentBuilder WithId(string id)
	{
		_id = id;
		return this;
	}

	public CommentBuilder WithTitle(string title)
	{
		_title = title;
		return this;
	}

	public CommentBuilder WithDescription(string description)
	{
		_description = description;
		return this;
	}

	public CommentBuilder WithCreatedAt(DateTime createdAt)
	{
		_dateCreated = createdAt;
		return this;
	}

	public CommentBuilder WithIssue(IssueDto issue)
	{
		_issue = issue;
		return this;
	}

	public CommentBuilder WithAuthor(UserDto author)
	{
		_author = author;
		return this;
	}

	public CommentBuilder WithModifiedAt(DateTime? modifiedAt)
	{
		_dateModified = modifiedAt;
		return this;
	}

	public CommentDto Build() =>
		new(
			ObjectId.Parse(_id),
			_title,
			_description,
			_dateCreated,
			_dateModified,
			_issue,
			_author,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

	public static CommentBuilder Default() => new();
}
