// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
namespace Shared.Contracts;

/// <summary>
/// Command for soft-deleting (archiving) a category.
/// </summary>
public record DeleteCategoryCommand
{
	/// <summary>
	/// Gets or sets the category ID.
	/// </summary>
	public ObjectId Id { get; init; }
}
