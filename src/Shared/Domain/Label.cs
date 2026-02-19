namespace IssueManager.Shared.Domain;

/// <summary>
/// Represents a label that can be attached to an issue for categorization.
/// </summary>
/// <param name="Name">The name of the label.</param>
/// <param name="Color">The color code for the label (hex format).</param>
public record Label(string Name, string Color)
{
	/// <summary>
	/// Gets the name of the label.
	/// </summary>
	public string Name { get; init; } = !string.IsNullOrWhiteSpace(Name)
		? Name
		: throw new ArgumentException("Label name cannot be empty.", nameof(Name));

	/// <summary>
	/// Gets the color code for the label.
	/// </summary>
	public string Color { get; init; } = !string.IsNullOrWhiteSpace(Color)
		? Color
		: throw new ArgumentException("Label color cannot be empty.", nameof(Color));
}
