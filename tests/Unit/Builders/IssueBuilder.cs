using IssueManager.Shared.Domain.Models;

namespace IssueManager.Tests.Unit.Builders;

/// <summary>
/// Builder for creating Issue test data with fluent API.
/// </summary>
public class IssueBuilder
{
	private string _id = Guid.NewGuid().ToString();
	private string _title = "Default Test Issue";
	private string _description = "Default test description";
	private string _authorId = "test-user-123";
	private DateTime _createdAt = DateTime.UtcNow;
	private DateTime? _updatedAt;
	private bool _isArchived;
	private string? _categoryId;
	private string? _statusId;
	private bool _approvedForRelease;
	private bool _rejected;

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
	/// Sets the author ID.
	/// </summary>
	public IssueBuilder WithAuthorId(string authorId)
	{
		_authorId = authorId;
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
	public IssueBuilder WithUpdatedAt(DateTime? updatedAt)
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
	/// Sets the category ID.
	/// </summary>
	public IssueBuilder WithCategoryId(string? categoryId)
	{
		_categoryId = categoryId;
		return this;
	}

	/// <summary>
	/// Sets the status ID.
	/// </summary>
	public IssueBuilder WithStatusId(string? statusId)
	{
		_statusId = statusId;
		return this;
	}

	/// <summary>
	/// Marks the issue as approved for release.
	/// </summary>
	public IssueBuilder AsApprovedForRelease()
	{
		_approvedForRelease = true;
		return this;
	}

	/// <summary>
	/// Marks the issue as rejected.
	/// </summary>
	public IssueBuilder AsRejected()
	{
		_rejected = true;
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
			AuthorId: _authorId,
			CreatedAt: _createdAt)
		{
			UpdatedAt = _updatedAt,
			IsArchived = _isArchived,
			CategoryId = _categoryId,
			StatusId = _statusId,
			ApprovedForRelease = _approvedForRelease,
			Rejected = _rejected
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
	/// Creates an issue builder with a specific title.
	/// </summary>
	public static IssueBuilder WithTitle(string title) => new IssueBuilder().WithTitle(title);
}
