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

	/// <summary>
	/// Gets or sets the search term for filtering by title or description.
	/// </summary>
	public string? SearchTerm { get; init; }

	/// <summary>
	/// Gets or sets the author name for filtering by author.
	/// </summary>
	public string? AuthorName { get; init; }
}
