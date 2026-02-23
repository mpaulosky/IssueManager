// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueBuilder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using MongoDB.Bson;
using Shared.DTOs;

namespace Tests.Unit.Builders;

/// <summary>
/// Builder for creating IssueDto test data with fluent API.
/// </summary>
public class IssueBuilder
{
	private ObjectId _id = ObjectId.GenerateNewId();
	private string _title = "Default Test Issue";
	private string _description = "Default test description";
	private StatusDto _status = new("Open", "Issue is open");
	private DateTime _dateCreated = DateTime.UtcNow;
	private bool _archived;

	public IssueBuilder WithId(string id)
	{
		_id = ObjectId.Parse(id);
		return this;
	}

	public IssueBuilder WithTitle(string title)
	{
		_title = title;
		return this;
	}

	public IssueBuilder WithDescription(string description)
	{
		_description = description;
		return this;
	}

	public IssueBuilder WithStatus(StatusDto status)
	{
		_status = status;
		return this;
	}

	public IssueBuilder WithCreatedAt(DateTime createdAt)
	{
		_dateCreated = createdAt;
		return this;
	}

	public IssueBuilder AsArchived()
	{
		_archived = true;
		return this;
	}

	public IssueBuilder AsActive()
	{
		_archived = false;
		return this;
	}

	public IssueDto Build() =>
		new(
			_id,
			_title,
			_description,
			_dateCreated,
			UserDto.Empty,
			CategoryDto.Empty,
			_status,
			_archived);

	public static IssueBuilder Default() => new();
	public static IssueBuilder Archived() => new IssueBuilder().AsArchived();
	public static IssueBuilder Closed() => new IssueBuilder().WithStatus(new StatusDto("Closed", "Issue is closed"));
}
