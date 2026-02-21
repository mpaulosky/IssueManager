using global::Shared.Domain;

namespace IssueManager.Tests.Unit.Builders;

/// <summary>
/// Builder for creating Issue test data with fluent API.
/// </summary>
public class IssueBuilder
{
	private string _id = Guid.NewGuid().ToString();
	private string _title = "Default Test Issue";
	private string _description = "Default test description";
	private IssueStatus _status = IssueStatus.Open;
	private DateTime _createdAt = DateTime.UtcNow;
	private DateTime _updatedAt = DateTime.UtcNow;
	private bool _isArchived;

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
	public IssueBuilder WithDescription(string description)
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
	/// Marks the issue as archived.
	/// </summary>
	public IssueBuilder AsArchived()
	{
		_isArchived = true;
		return this;
	}

	/// <summary>
	/// Marks the issue as not archived.
	/// </summary>
	public IssueBuilder AsActive()
	{
		_isArchived = false;
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
			UpdatedAt: _updatedAt)
		{
			IsArchived = _isArchived
		};
	}

	/// <summary>
	/// Creates a default issue builder.
	/// </summary>
	public static IssueBuilder Default() => new();

	/// <summary>
	/// Creates an archived issue builder.
	/// </summary>
	public static IssueBuilder Archived() => new IssueBuilder().AsArchived();

	/// <summary>
	/// Creates a closed issue builder.
	/// </summary>
	public static IssueBuilder Closed() => new IssueBuilder().WithStatus(IssueStatus.Closed);
}
