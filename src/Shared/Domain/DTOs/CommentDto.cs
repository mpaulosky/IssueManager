namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
///   Data transfer object for Comment.
/// </summary>
/// <param name="Id">The unique identifier for the comment.</param>
/// <param name="Title">The title of the comment.</param>
/// <param name="Description">The description of the comment.</param>
/// <param name="DateCreated">The date the comment was created.</param>
/// <param name="Issue">The issue this comment belongs to.</param>
/// <param name="Author">The author of the comment.</param>
/// <param name="UserVotes">The collection of user IDs who have voted.</param>
/// <param name="Archived">Indicates whether the comment is archived.</param>
/// <param name="ArchivedBy">The user who archived the comment.</param>
/// <param name="IsAnswer">Indicates whether this comment is the selected answer.</param>
/// <param name="AnswerSelectedBy">The user who selected this as the answer.</param>
public record CommentDto(
	ObjectId Id,
	string Title,
	string Description,
	DateTime DateCreated,
	IssueDto? Issue,
	UserDto Author,
	HashSet<string> UserVotes,
	bool Archived,
	UserDto? ArchivedBy,
	bool IsAnswer,
	UserDto? AnswerSelectedBy)
{
	/// <summary>
	///   Gets an empty CommentDto instance.
	/// </summary>
	public static CommentDto Empty => new(
		ObjectId.Empty,
		string.Empty,
		string.Empty,
		DateTime.UtcNow,
		null,
		UserDto.Empty,
		new HashSet<string>(),
		false,
		null,
		false,
		null);
}
