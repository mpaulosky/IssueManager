namespace Shared.Domain.Enums;

/// <summary>
///   Represents the lifecycle status of an issue.
/// </summary>
public enum IssueStatus
{
	/// <summary>
	///   The issue has been newly created and not yet started.
	/// </summary>
	New,

	/// <summary>
	///   The issue is currently being worked on.
	/// </summary>
	InProgress,

	/// <summary>
	///   The issue has been resolved but not yet closed.
	/// </summary>
	Resolved,

	/// <summary>
	///   The issue has been closed and is no longer active.
	/// </summary>
	Closed
}
