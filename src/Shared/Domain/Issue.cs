namespace IssueManager.Shared.Domain;

/// <summary>
/// Represents an issue in the issue tracking system.
/// </summary>
/// <param name="Id">The unique identifier for the issue.</param>
/// <param name="Title">The title of the issue.</param>
/// <param name="Description">The detailed description of the issue.</param>
/// <param name="Status">The current status of the issue.</param>
/// <param name="CreatedAt">The timestamp when the issue was created.</param>
/// <param name="UpdatedAt">The timestamp when the issue was last updated.</param>
/// <param name="Labels">The collection of labels attached to the issue.</param>
public record Issue(
	string Id,
	string Title,
	string? Description,
	IssueStatus Status,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	IReadOnlyCollection<Label>? Labels = null)
{
	/// <summary>
	/// Gets the unique identifier for the issue.
	/// </summary>
	public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
		? Id
		: throw new ArgumentException("Issue ID cannot be empty.", nameof(Id));

	/// <summary>
	/// Gets the title of the issue.
	/// </summary>
	public string Title { get; init; } = !string.IsNullOrWhiteSpace(Title)
		? Title
		: throw new ArgumentException("Issue title cannot be empty.", nameof(Title));

	/// <summary>
	/// Gets the collection of labels attached to the issue.
	/// </summary>
	public IReadOnlyCollection<Label> Labels { get; init; } = Labels ?? Array.Empty<Label>();

	/// <summary>
	/// Creates a new issue with the specified properties.
	/// </summary>
	public static Issue Create(string title, string? description, IReadOnlyCollection<Label>? labels = null)
	{
		return new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: title,
			Description: description,
			Status: IssueStatus.Open,
			CreatedAt: DateTime.UtcNow,
			UpdatedAt: DateTime.UtcNow,
			Labels: labels);
	}

	/// <summary>
	/// Updates the status of the issue.
	/// </summary>
	public Issue UpdateStatus(IssueStatus newStatus)
	{
		if (Status == newStatus)
			return this;

		return this with
		{
			Status = newStatus,
			UpdatedAt = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Updates the title and description of the issue.
	/// </summary>
	public Issue Update(string title, string? description)
	{
		return this with
		{
			Title = title,
			Description = description,
			UpdatedAt = DateTime.UtcNow
		};
	}
}
