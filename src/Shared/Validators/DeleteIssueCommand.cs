// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
namespace Shared.Validators;

/// <summary>
/// Command for soft-deleting (archiving) an issue.
/// </summary>
public record DeleteIssueCommand
{
	/// <summary>
	/// Gets or sets the issue ID.
	/// </summary>
	public string Id { get; init; } = string.Empty;
}
