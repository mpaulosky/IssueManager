namespace IssueManager.Shared.Domain.Models;

/// <summary>
///   Status class for tracking issue lifecycle (e.g., Open, In Progress, Resolved, Closed).
/// </summary>
[Serializable]
public class Status
{
	/// <summary>
	///   Gets or sets the unique identifier for the status.
	/// </summary>
	/// <value>
	///   The unique identifier.
	/// </value>
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public ObjectId Id { get; set; } = ObjectId.Empty;

	/// <summary>
	///   Gets or sets the name of the status.
	/// </summary>
	/// <value>
	///   The name of the status.
	/// </value>
	[BsonElement("status_name")]
	[BsonRepresentation(BsonType.String)]
	public string StatusName { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the status description.
	/// </summary>
	/// <value>
	///   The status description.
	/// </value>
	[BsonElement("status_description")]
	[BsonRepresentation(BsonType.String)]
	public string StatusDescription { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets a value indicating whether this <see cref="Status" /> is archived.
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
}
