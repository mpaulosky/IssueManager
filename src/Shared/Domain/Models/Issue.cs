namespace IssueManager.Shared.Domain.Models;

/// <summary>
///   Issue class representing the main entity for tracking issues.
/// </summary>
[Serializable]
public class Issue
{
	/// <summary>
	///   Gets or sets the unique identifier for the issue.
	/// </summary>
	/// <value>
	///   The unique identifier.
	/// </value>
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the title of the issue.
	/// </summary>
	/// <value>
	///   The title.
	/// </value>
	[BsonElement("issue_title")]
	[BsonRepresentation(BsonType.String)]
	public string Title { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the description of the issue.
	/// </summary>
	/// <value>
	///   The description.
	/// </value>
	[BsonElement("issue_description")]
	[BsonRepresentation(BsonType.String)]
	public string Description { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the date the issue was created.
	/// </summary>
	/// <value>
	///   The date created.
	/// </value>
	[BsonElement("date_created")]
	[BsonRepresentation(BsonType.DateTime)]
	public DateTime DateCreated { get; init; } = DateTime.UtcNow;

	/// <summary>
	///   Gets or sets the category of the issue.
	/// </summary>
	/// <value>
	///   The category.
	/// </value>
	public CategoryDto Category { get; set; } = CategoryDto.Empty;

	/// <summary>
	///   Gets or sets the author of the issue.
	/// </summary>
	/// <value>
	///   The author.
	/// </value>
	public UserDto Author { get; set; } = UserDto.Empty;

	/// <summary>
	///   Gets or sets the status of the issue.
	/// </summary>
	/// <value>
	///   The issue status.
	/// </value>
	public StatusDto IssueStatus { get; set; } = StatusDto.Empty;

	/// <summary>
	///   Gets or sets a value indicating whether this <see cref="Issue" /> is archived.
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
	///   Gets or sets a value indicating whether this issue is approved for release.
	/// </summary>
	/// <value>
	///   <c>true</c> if approved for release; otherwise, <c>false</c>.
	/// </value>
	[BsonElement("approved_for_release")]
	[BsonRepresentation(BsonType.Boolean)]
	public bool ApprovedForRelease { get; set; }

	/// <summary>
	///   Gets or sets a value indicating whether this <see cref="Issue" /> is rejected.
	/// </summary>
	/// <value>
	///   <c>true</c> if rejected; otherwise, <c>false</c>.
	/// </value>
	[BsonElement("rejected")]
	[BsonRepresentation(BsonType.Boolean)]
	public bool Rejected { get; set; }
}
