namespace IssueManager.Shared.Domain.Models;

/// <summary>
/// Represents a category for organizing and classifying issues.
/// </summary>
/// <param name="Id">The unique identifier for the category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="Description">The description of the category.</param>
public record Category(string Id, string Name, string Description)
{
	/// <summary>
	/// Gets the unique identifier for the category.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("Category ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the name of the category.
	/// </summary>
	public string Name { get; init; } = !string.IsNullOrWhiteSpace(Name)
		? Name
		: throw new ArgumentException("Category name cannot be empty.", nameof(Name));

	/// <summary>
	/// Gets the description of the category.
	/// </summary>
	public string Description { get; init; } = !string.IsNullOrWhiteSpace(Description)
		? Description
		: throw new ArgumentException("Category description cannot be empty.", nameof(Description));
}
