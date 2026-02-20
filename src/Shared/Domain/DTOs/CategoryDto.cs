namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
///   Data transfer object for Category.
/// </summary>
/// <param name="Id">The unique identifier for the category.</param>
/// <param name="CategoryName">The name of the category.</param>
/// <param name="CategoryDescription">The description of the category.</param>
/// <param name="Archived">Indicates whether the category is archived.</param>
/// <param name="ArchivedBy">The user who archived the category.</param>
public record CategoryDto(
	string? Id,
	string CategoryName,
	string CategoryDescription,
	bool Archived,
	UserDto? ArchivedBy)
{
	/// <summary>
	///   Gets an empty CategoryDto instance.
	/// </summary>
	public static CategoryDto Empty => new(
		string.Empty,
		string.Empty,
		string.Empty,
		false,
		null);
}
