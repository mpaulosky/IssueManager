namespace IssueManager.Shared.Domain.Models;

/// <summary>
///   User class representing Auth0 user information (not stored in database, used as DTO).
/// </summary>
[Serializable]
public class User
{
	/// <summary>
	///   Gets or sets the unique identifier for the user.
	/// </summary>
	/// <value>
	///   The unique identifier.
	/// </value>
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the display name of the user.
	/// </summary>
	/// <value>
	///   The display name.
	/// </value>
	[BsonElement("display_name")]
	[BsonRepresentation(BsonType.String)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the email address of the user.
	/// </summary>
	/// <value>
	///   The email address.
	/// </value>
	[BsonElement("email_address")]
	[BsonRepresentation(BsonType.String)]
	public string Email { get; set; } = string.Empty;
}
