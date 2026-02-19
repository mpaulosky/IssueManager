namespace IssueManager.Shared.Domain.Models;

/// <summary>
/// Represents a status for tracking the lifecycle of an issue.
/// </summary>
/// <param name="Id">The unique identifier for the status.</param>
/// <param name="Name">The name of the status.</param>
/// <param name="Description">The description of the status.</param>
public record Status(string Id, string Name, string Description)
{
	/// <summary>
	/// Gets the unique identifier for the status.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("Status ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the name of the status.
	/// </summary>
	public string Name { get; init; } = !string.IsNullOrWhiteSpace(Name)
		? Name
		: throw new ArgumentException("Status name cannot be empty.", nameof(Name));

	/// <summary>
	/// Gets the description of the status.
	/// </summary>
	public string Description { get; init; } = !string.IsNullOrWhiteSpace(Description)
		? Description
		: throw new ArgumentException("Status description cannot be empty.", nameof(Description));
}
