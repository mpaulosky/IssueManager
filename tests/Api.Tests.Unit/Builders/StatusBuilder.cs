// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusBuilder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Api.Builders;

/// <summary>
/// Builder for creating StatusDto test data with fluent API.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusBuilder
{
	private string? _id = ObjectId.GenerateNewId().ToString();
	private string _statusName = "Default Status";
	private string _statusDescription = "Default status description";
	private DateTime _dateCreated = DateTime.UtcNow;
	private DateTime? _dateModified;

	public StatusBuilder WithId(string id)
	{
		_id = id;
		return this;
	}

	public StatusBuilder WithStatusName(string statusName)
	{
		_statusName = statusName;
		return this;
	}

	public StatusBuilder WithStatusDescription(string statusDescription)
	{
		_statusDescription = statusDescription;
		return this;
	}

	public StatusBuilder WithCreatedAt(DateTime createdAt)
	{
		_dateCreated = createdAt;
		return this;
	}

	public StatusBuilder WithModifiedAt(DateTime? modifiedAt)
	{
		_dateModified = modifiedAt;
		return this;
	}

	public StatusDto Build() =>
		new(
			ObjectId.Parse(_id ?? ObjectId.GenerateNewId().ToString()),
			_statusName,
			_statusDescription,
			_dateCreated,
			_dateModified,
			false,
			UserDto.Empty);

	public static StatusBuilder Default() => new();
}
