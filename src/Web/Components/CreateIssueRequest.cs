using System.ComponentModel.DataAnnotations;
using IssueManager.Shared.Domain;

namespace IssueManager.Web.Components;

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
	public IssueStatus Status { get; set; } = IssueStatus.Open;
}
