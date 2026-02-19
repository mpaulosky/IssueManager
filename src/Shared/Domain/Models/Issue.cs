namespace IssueManager.Shared.Domain.Models;

/// <summary>
/// Represents an issue in the issue tracking system.
/// </summary>
/// <param name="Id">The unique identifier for the issue.</param>
/// <param name="Title">The title of the issue.</param>
/// <param name="Description">The description of the issue.</param>
/// <param name="AuthorId">The ID of the user who created the issue.</param>
/// <param name="CreatedAt">The timestamp when the issue was created.</param>
public record Issue(string Id, string Title, string Description, string AuthorId, DateTime CreatedAt)
{
	/// <summary>
	/// Gets the unique identifier for the issue.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("Issue ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the title of the issue.
	/// </summary>
	public string Title { get; init; } = !string.IsNullOrWhiteSpace(Title)
		? Title
		: throw new ArgumentException("Issue title cannot be empty.", nameof(Title));

	/// <summary>
	/// Gets the description of the issue.
	/// </summary>
	public string Description { get; init; } = !string.IsNullOrWhiteSpace(Description)
		? Description
		: throw new ArgumentException("Issue description cannot be empty.", nameof(Description));

	/// <summary>
	/// Gets the ID of the user who created the issue.
	/// </summary>
	public string AuthorId { get; init; } = !string.IsNullOrWhiteSpace(AuthorId)
		? AuthorId
		: throw new ArgumentException("Author ID cannot be empty.", nameof(AuthorId));

	/// <summary>
	/// Gets the timestamp when the issue was created.
	/// </summary>
	public DateTime CreatedAt { get; init; } = CreatedAt != default ? CreatedAt : throw new ArgumentException("Created date cannot be default.", nameof(CreatedAt));

	/// <summary>
	/// Gets the ID of the category.
	/// </summary>
	public string? CategoryId { get; init; }

	/// <summary>
	/// Gets the ID of the status.
	/// </summary>
	public string? StatusId { get; init; }

	/// <summary>
	/// Gets the timestamp when the issue was last updated.
	/// </summary>
	public DateTime? UpdatedAt { get; init; }

	/// <summary>
	/// Gets a value indicating whether this issue is archived.
	/// </summary>
	public bool IsArchived { get; init; }

	/// <summary>
	/// Gets a value indicating whether this issue is approved for release.
	/// </summary>
	public bool ApprovedForRelease { get; init; }

	/// <summary>
	/// Gets a value indicating whether this issue is rejected.
	/// </summary>
	public bool Rejected { get; init; }
}
