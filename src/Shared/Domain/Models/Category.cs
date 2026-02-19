namespace IssueManager.Shared.Domain.Models;

/// <summary>
///   Category class for categorizing issues (e.g., Bug, Feature Request, Documentation).
/// </summary>
[Serializable]
public class Category
{
	/// <summary>
	///   Gets or sets the unique identifier for the category.
	/// </summary>
	/// <value>
	///   The unique identifier.
	/// </value>
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
	public ObjectId? Id { get; set; } = ObjectId.Empty;

	/// <summary>
	///   Gets or sets the name of the category.
	/// </summary>
	/// <value>
	///   The name of the category.
	/// </value>
	[BsonElement("category_name")]
	[BsonRepresentation(BsonType.String)]
	public string CategoryName { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the category description.
	/// </summary>
	/// <value>
	///   The category description.
	/// </value>
	[BsonElement("category_description")]
	[BsonRepresentation(BsonType.String)]
	public string CategoryDescription { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets a value indicating whether this <see cref="Category" /> is archived.
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
