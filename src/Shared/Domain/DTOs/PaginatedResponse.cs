namespace IssueManager.Shared.Domain.DTOs;

/// <summary>
/// Represents a paginated response containing a list of items.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public record PaginatedResponse<T>
{
	/// <summary>
	/// Gets the list of items for the current page.
	/// </summary>
	public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

	/// <summary>
	/// Gets the total number of items across all pages.
	/// </summary>
	public long Total { get; init; }

	/// <summary>
	/// Gets the current page number (1-indexed).
	/// </summary>
	public int Page { get; init; }

	/// <summary>
	/// Gets the number of items per page.
	/// </summary>
	public int PageSize { get; init; }

	/// <summary>
	/// Gets the total number of pages.
	/// </summary>
	public int TotalPages { get; init; }
}
