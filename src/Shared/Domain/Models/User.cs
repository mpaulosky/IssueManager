namespace IssueManager.Shared.Domain.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
/// <param name="Id">The unique identifier for the user.</param>
/// <param name="Name">The display name of the user.</param>
/// <param name="Email">The email address of the user.</param>
public record User(string Id, string Name, string Email)
{
	/// <summary>
	/// Gets the unique identifier for the user.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("User ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the display name of the user.
	/// </summary>
	public string Name { get; init; } = !string.IsNullOrWhiteSpace(Name)
		? Name
		: throw new ArgumentException("User name cannot be empty.", nameof(Name));

	/// <summary>
	/// Gets the email address of the user.
	/// </summary>
	public string Email { get; init; } = !string.IsNullOrWhiteSpace(Email)
		? Email
		: throw new ArgumentException("User email cannot be empty.", nameof(Email));
}
