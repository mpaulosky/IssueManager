// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueRequest.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =======================================================
using System.ComponentModel.DataAnnotations;

namespace Web.Components;

/// <summary>
/// Request model for creating or updating an issue.
/// </summary>
public record CreateIssueRequest
{
	/// <summary>
	/// Gets or sets the title of the issue.
	/// </summary>
	[Required(ErrorMessage = "Title is required.")]
	[StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the issue.
	/// </summary>
	[StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the status of the issue.
	/// </summary>
	public string Status { get; set; } = "Open";

	/// <summary>
	/// Gets or sets the category ID for the issue.
	/// </summary>
	public string? CategoryId { get; set; }

	/// <summary>
	/// Gets or sets the status ID for the issue.
	/// </summary>
	public string? StatusId { get; set; }
}
