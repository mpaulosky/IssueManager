namespace Shared.DTOs;

public record LabelDto(string Name, string Color)
{
	/// <summary>
	///   Gets an empty LabelDto instance.
	/// </summary>
	public static LabelDto Empty => new(string.Empty, string.Empty);
}