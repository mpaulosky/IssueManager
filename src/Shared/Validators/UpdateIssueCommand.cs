// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
namespace Shared.Validators;

/// <summary>
/// Command for updating an existing issue.
/// </summary>
public record UpdateIssueCommand
{
	/// <summary>
	/// Gets or sets the issue ID.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	/// Gets or sets the title of the issue.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the issue.
	/// </summary>
	public string? Description { get; init; }
}
