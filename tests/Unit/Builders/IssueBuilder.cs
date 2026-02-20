using Shared.Domain;

namespace IssueManager.Tests.Unit.Builders;

/// <summary>
/// Builder for creating Issue test data with fluent API.
/// </summary>
public class IssueBuilder
{
	private string _id = Guid.NewGuid().ToString();
	private string _title = "Default Test Issue";
	private string? _description = "Default test description";
	private IssueStatus _status = IssueStatus.Open;
	private DateTime _createdAt = DateTime.UtcNow;
	private DateTime _updatedAt = DateTime.UtcNow;
	private IReadOnlyCollection<Label>? _labels;

	/// <summary>
	/// Sets the issue ID.
	/// </summary>
	public IssueBuilder WithId(string id)
	{
		_id = id;
		return this;
	}

	/// <summary>
	/// Sets the issue title.
	/// </summary>
	public IssueBuilder WithTitle(string title)
	{
		_title = title;
		return this;
	}

	/// <summary>
	/// Sets the issue description.
	/// </summary>
	public IssueBuilder WithDescription(string? description)
	{
		_description = description;
		return this;
	}

	/// <summary>
	/// Sets the issue status.
	/// </summary>
	public IssueBuilder WithStatus(IssueStatus status)
	{
		_status = status;
		return this;
	}

	/// <summary>
	/// Sets the created at timestamp.
	/// </summary>
	public IssueBuilder WithCreatedAt(DateTime createdAt)
	{
		_createdAt = createdAt;
		return this;
	}

	/// <summary>
	/// Sets the updated at timestamp.
	/// </summary>
	public IssueBuilder WithUpdatedAt(DateTime updatedAt)
	{
		_updatedAt = updatedAt;
		return this;
	}

	/// <summary>
	/// Sets the labels collection.
	/// </summary>
	public IssueBuilder WithLabels(IReadOnlyCollection<Label>? labels)
	{
		_labels = labels;
		return this;
	}

	/// <summary>
	/// Builds the Issue instance.
	/// </summary>
	public Issue Build()
	{
		return new Issue(
			Id: _id,
			Title: _title,
			Description: _description,
			Status: _status,
			CreatedAt: _createdAt,
			UpdatedAt: _updatedAt,
			Labels: _labels);
	}

	/// <summary>
	/// Creates a default issue builder.
	/// </summary>
	public static IssueBuilder Default() => new();

	/// <summary>
	/// Creates a closed issue builder.
	/// </summary>
	public static IssueBuilder Closed() => new IssueBuilder().WithStatus(IssueStatus.Closed);
}
