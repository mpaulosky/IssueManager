namespace Shared.Validators;

/// <summary>
/// Query for retrieving a paginated list of issues.
/// </summary>
public record ListIssuesQuery
{
	/// <summary>
	/// Gets or sets the page number (1-indexed).
	/// </summary>
	public int Page { get; init; } = 1;

	/// <summary>
	/// Gets or sets the page size.
	/// </summary>
	public int PageSize { get; init; } = 20;
}
