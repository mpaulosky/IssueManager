namespace IssueManager.Shared.Domain;

/// <summary>
/// Represents the possible states of an issue.
/// </summary>
public enum IssueStatus
{
	/// <summary>Issue is newly created and not yet worked on.</summary>
	Open,

	/// <summary>Issue is being actively worked on.</summary>
	InProgress,

	/// <summary>Issue has been completed and closed.</summary>
	Closed
}
