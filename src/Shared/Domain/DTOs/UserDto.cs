namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
///   Data transfer object for User (Auth0 user information).
/// </summary>
/// <param name="Id">The unique identifier for the user.</param>
/// <param name="Name">The display name of the user.</param>
/// <param name="Email">The email address of the user.</param>
public record UserDto(
	string Id,
	string Name,
	string Email)
{
	/// <summary>
	///   Gets an empty UserDto instance.
	/// </summary>
	public static UserDto Empty => new(
		string.Empty,
		string.Empty,
		string.Empty);
}
