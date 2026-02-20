namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
///   Data transfer object for Status.
/// </summary>
/// <param name="Id">The unique identifier for the status.</param>
/// <param name="StatusName">The name of the status.</param>
/// <param name="StatusDescription">The description of the status.</param>
/// <param name="Archived">Indicates whether the status is archived.</param>
/// <param name="ArchivedBy">The user who archived the status.</param>
public record StatusDto(
	string Id,
	string StatusName,
	string StatusDescription,
	bool Archived,
	UserDto? ArchivedBy)
{
	/// <summary>
	///   Gets an empty StatusDto instance.
	/// </summary>
	public static StatusDto Empty => new(
		string.Empty,
		string.Empty,
		string.Empty,
		false,
		null);
}
