// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
namespace Shared.Contracts;

/// <summary>
/// Command for soft-deleting (archiving) a status.
/// </summary>
public record DeleteStatusCommand
{
	/// <summary>
	/// Gets or sets the status ID.
	/// </summary>
	public ObjectId Id { get; init; }
}
