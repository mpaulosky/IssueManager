namespace IssueManager.Shared.Domain.Models;

/// <summary>
///   Comment class for issue discussions with threaded support, voting, and answer marking.
/// </summary>
[Serializable]
public class Comment
{
	/// <summary>
	///   Gets or sets the unique identifier for the comment.
	/// </summary>
	/// <value>
	///   The unique identifier.
	/// </value>
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public ObjectId Id { get; set; } = ObjectId.Empty;

	/// <summary>
	///   Gets or sets the title of the comment.
	/// </summary>
	/// <value>
	///   The title.
	/// </value>
	[BsonElement("comment_title")]
	[BsonRepresentation(BsonType.String)]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the description of the comment.
	/// </summary>
	/// <value>
	///   The description.
	/// </value>
	[BsonElement("comment_description")]
	[BsonRepresentation(BsonType.String)]
	public string Description { get; init; } = string.Empty;

	/// <summary>
	///   Gets or sets the date the comment was created.
	/// </summary>
	/// <value>
	///   The date created.
	/// </value>
	[BsonElement("date_created")]
	[BsonRepresentation(BsonType.DateTime)]
	public DateTime DateCreated { get; init; } = DateTime.UtcNow;

	/// <summary>
	///   Gets or sets the issue this comment belongs to.
	/// </summary>
	/// <value>
	///   The associated issue.
	/// </value>
	public IssueDto Issue { get; set; } = IssueDto.Empty;

	/// <summary>
	///   Gets or sets the author of the comment.
	/// </summary>
	/// <value>
	///   The author.
	/// </value>
	public UserDto Author { get; set; } = UserDto.Empty;

	/// <summary>
	///   Gets or sets the user votes for this comment.
	/// </summary>
	/// <value>
	///   The collection of user IDs who have voted.
	/// </value>
	public HashSet<string> UserVotes { get; init; } = new();

	/// <summary>
	///   Gets or sets a value indicating whether this <see cref="Comment" /> is archived.
	/// </summary>
	/// <value>
	///   <c>true</c> if archived; otherwise, <c>false</c>.
	/// </value>
	[BsonElement("archived")]
	[BsonRepresentation(BsonType.Boolean)]
	public bool Archived { get; set; }

	/// <summary>
	///   Gets or sets who archived the record.
	/// </summary>
	/// <value>
	///   The user who archived the record.
	/// </value>
	public UserDto ArchivedBy { get; set; } = UserDto.Empty;

	/// <summary>
	///   Gets or sets a value indicating whether this comment is the selected answer to the associated issue.
	/// </summary>
	/// <value>
	///   <c>true</c> if this is the answer; otherwise, <c>false</c>.
	/// </value>
	[BsonElement("is_answer")]
	[BsonRepresentation(BsonType.Boolean)]
	public bool IsAnswer { get; set; }

	/// <summary>
	///   Gets or sets the user who selected this comment as the answer to the associated issue.
	/// </summary>
	/// <value>
	///   The user who selected this as the answer.
	/// </value>
	public UserDto AnswerSelectedBy { get; set; } = UserDto.Empty;
}
