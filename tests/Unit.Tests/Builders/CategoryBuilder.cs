// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryBuilder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using MongoDB.Bson;
using Shared.DTOs;

namespace Tests.Unit.Builders;

/// <summary>
/// Builder for creating CategoryDto test data with fluent API.
/// </summary>
public class CategoryBuilder
{
	private ObjectId _id = ObjectId.GenerateNewId();
	private string _categoryName = "Default Category";
	private string _categoryDescription = "Default category description";
	private DateTime _dateCreated = DateTime.UtcNow;
	private DateTime? _dateModified;

	public CategoryBuilder WithId(string id)
	{
		_id = ObjectId.Parse(id);
		return this;
	}

	public CategoryBuilder WithCategoryName(string categoryName)
	{
		_categoryName = categoryName;
		return this;
	}

	public CategoryBuilder WithCategoryDescription(string categoryDescription)
	{
		_categoryDescription = categoryDescription;
		return this;
	}

	public CategoryBuilder WithCreatedAt(DateTime createdAt)
	{
		_dateCreated = createdAt;
		return this;
	}

	public CategoryBuilder WithModifiedAt(DateTime? modifiedAt)
	{
		_dateModified = modifiedAt;
		return this;
	}

	public CategoryDto Build() =>
		new(
			_id,
			_categoryName,
			_categoryDescription,
			_dateCreated,
			_dateModified);

	public static CategoryBuilder Default() => new();
}
