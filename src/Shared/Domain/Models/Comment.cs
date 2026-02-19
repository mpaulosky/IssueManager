namespace IssueManager.Shared.Domain.Models;

/// <summary>
/// Represents a comment on an issue.
/// </summary>
/// <param name="Id">The unique identifier for the comment.</param>
/// <param name="Title">The title of the comment.</param>
/// <param name="Description">The description of the comment.</param>
/// <param name="IssueId">The ID of the issue this comment belongs to.</param>
/// <param name="AuthorId">The ID of the user who created the comment.</param>
/// <param name="CreatedAt">The timestamp when the comment was created.</param>
public record Comment(
	string Id,
	string Title,
	string Description,
	string IssueId,
	string AuthorId,
	DateTime CreatedAt)
{
	/// <summary>
	/// Gets the unique identifier for the comment.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("Comment ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the title of the comment.
	/// </summary>
	public string Title { get; init; } = !string.IsNullOrWhiteSpace(Title)
		? Title
		: throw new ArgumentException("Comment title cannot be empty.", nameof(Title));

	/// <summary>
	/// Gets the description of the comment.
	/// </summary>
	public string Description { get; init; } = !string.IsNullOrWhiteSpace(Description)
		? Description
		: throw new ArgumentException("Comment description cannot be empty.", nameof(Description));

	/// <summary>
	/// Gets the ID of the issue this comment belongs to.
	/// </summary>
	public string IssueId { get; init; } = !string.IsNullOrWhiteSpace(IssueId)
		? IssueId
		: throw new ArgumentException("Issue ID cannot be empty.", nameof(IssueId));

	/// <summary>
	/// Gets the ID of the user who created the comment.
	/// </summary>
	public string AuthorId { get; init; } = !string.IsNullOrWhiteSpace(AuthorId)
		? AuthorId
		: throw new ArgumentException("Author ID cannot be empty.", nameof(AuthorId));

	/// <summary>
	/// Gets the timestamp when the comment was created.
	/// </summary>
	public DateTime CreatedAt { get; init; } = CreatedAt != default ? CreatedAt : throw new ArgumentException("Created date cannot be default.", nameof(CreatedAt));

	/// <summary>
	/// Gets the user votes for this comment.
	/// </summary>
	public IReadOnlySet<string> UserVotes { get; init; } = new HashSet<string>();

	/// <summary>
	/// Gets a value indicating whether this comment is the selected answer.
	/// </summary>
	public bool IsAnswer { get; init; }

	/// <summary>
	/// Gets the ID of the user who selected this as the answer, if applicable.
	/// </summary>
	public string? AnswerSelectedById { get; init; }
}
