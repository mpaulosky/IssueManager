namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
///   Data transfer object for Issue.
/// </summary>
/// <param name="Id">The unique identifier for the issue.</param>
/// <param name="Title">The title of the issue.</param>
/// <param name="Description">The description of the issue.</param>
/// <param name="DateCreated">The date the issue was created.</param>
/// <param name="Category">The category of the issue.</param>
/// <param name="Author">The author of the issue.</param>
/// <param name="IssueStatus">The status of the issue.</param>
/// <param name="Archived">Indicates whether the issue is archived.</param>
/// <param name="ArchivedBy">The user who archived the issue.</param>
/// <param name="ApprovedForRelease">Indicates whether the issue is approved for release.</param>
/// <param name="Rejected">Indicates whether the issue is rejected.</param>
public record IssueDto(
	string Id,
	string Title,
	string Description,
	DateTime DateCreated,
	CategoryDto Category,
	UserDto Author,
	StatusDto IssueStatus,
	bool Archived,
	UserDto? ArchivedBy,
	bool ApprovedForRelease,
	bool Rejected)
{
	/// <summary>
	///   Gets an empty IssueDto instance.
	/// </summary>
	public static IssueDto Empty => new(
		string.Empty,
		string.Empty,
		string.Empty,
		DateTime.UtcNow,
		CategoryDto.Empty,
		UserDto.Empty,
		StatusDto.Empty,
		false,
		null,
		false,
		false);
}
