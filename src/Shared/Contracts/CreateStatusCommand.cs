// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

namespace Shared.Contracts;

/// <summary>
/// Command for creating a new status. Validated by the Api project's generic
/// TaxonomyCommandValidator, configured through StatusTaxonomyAdapter.
/// </summary>
public record CreateStatusCommand
{
	/// <summary>
	/// Gets or sets the name of the status.
	/// </summary>
	public string StatusName { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the status.
	/// </summary>
	public string? StatusDescription { get; init; }
}
