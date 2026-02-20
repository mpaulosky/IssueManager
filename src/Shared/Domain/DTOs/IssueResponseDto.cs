namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
/// Simplified data transfer object for Issue (for list and single responses).
/// </summary>
public record IssueResponseDto
{
	/// <summary>
	/// Gets or sets the unique identifier for the issue.
	/// </summary>
	public string Id { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the title of the issue.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the issue.
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// Gets or sets the status of the issue.
	/// </summary>
	public string Status { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the timestamp when the issue was created.
	/// </summary>
	public DateTime CreatedAt { get; init; }

	/// <summary>
	/// Gets or sets the timestamp when the issue was last updated.
	/// </summary>
	public DateTime UpdatedAt { get; init; }

	/// <summary>
	/// Gets or sets the labels attached to the issue.
	/// </summary>
	public List<string>? Labels { get; init; }
}
