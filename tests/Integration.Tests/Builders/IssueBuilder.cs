namespace Integration.Builders;

/// <summary>
/// Builder for creating IssueDto test data with fluent API.
/// </summary>
public class IssueBuilder
{
	private ObjectId _id = ObjectId.GenerateNewId();
	private string _title = "Default Test Issue";
	private string _description = "Default test description";
	private DateTime _dateCreated = DateTime.UtcNow;
	private DateTime? _dateUpdated;
	private bool _archived;

	/// <summary>
	/// Sets the issue ID.
	/// </summary>
	public IssueBuilder WithId(ObjectId id)
	{
		_id = id;
		return this;
	}

	/// <summary>
	/// Sets the issue ID from string.
	/// </summary>
	public IssueBuilder WithId(string id)
	{
		if (ObjectId.TryParse(id, out var objectId))
		{
			_id = objectId;
		}
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
	/// Sets the created at timestamp.
	/// </summary>
	public IssueBuilder WithDateCreated(DateTime dateCreated)
	{
		_dateCreated = dateCreated;
		return this;
	}

	/// <summary>
	/// Sets the updated at timestamp.
	/// </summary>
	public IssueBuilder WithDateUpdated(DateTime? dateUpdated)
	{
		_dateUpdated = dateUpdated;
		return this;
	}

	/// <summary>
	/// Marks the issue as archived.
	/// </summary>
	public IssueBuilder AsArchived()
	{
		_archived = true;
		return this;
	}

	/// <summary>
	/// Marks the issue as not archived.
	/// </summary>
	public IssueBuilder AsActive()
	{
		_archived = false;
		return this;
	}

	/// <summary>
	/// Builds the IssueDto instance.
	/// </summary>
	public IssueDto Build()
	{
		return new IssueDto(
			Id: _id,
			Title: _title,
			Description: _description,
			DateCreated: _dateCreated,
			DateModified: _dateUpdated,
			Author: UserDto.Empty,
			Category: CategoryDto.Empty,
			Status: StatusDto.Empty,
			Archived: _archived,
			ArchivedBy: UserDto.Empty,
			ApprovedForRelease: false,
			Rejected: false);
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
	/// Creates a minimal active issue builder.
	/// </summary>
	public static IssueBuilder Active() => new IssueBuilder().AsActive();
}
