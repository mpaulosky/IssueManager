namespace Shared.Validators;

/// <summary>
/// Command for soft-deleting (archiving) an issue.
/// </summary>
public record DeleteIssueCommand
{
	/// <summary>
	/// Gets or sets the issue ID.
	/// </summary>
	public string Id { get; init; } = string.Empty;
}
