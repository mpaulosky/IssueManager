using Shared.Domain;

namespace IssueManager.Api.Data;

/// <summary>
/// Repository interface for issue persistence operations.
/// </summary>
public interface IIssueRepository
{
	/// <summary>
	/// Creates a new issue in the database.
	/// </summary>
	Task<Issue> CreateAsync(Issue issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an issue by its unique identifier.
	/// </summary>
	Task<Issue?> GetByIdAsync(string issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing issue in the database.
	/// </summary>
	Task<Issue?> UpdateAsync(Issue issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an issue from the database.
	/// </summary>
	Task<bool> DeleteAsync(string issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all issues from the database.
	/// </summary>
	Task<IReadOnlyList<Issue>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated issues from the database, excluding archived issues by default.
	/// </summary>
	Task<(IReadOnlyList<Issue> Items, long Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Soft-deletes an issue by setting IsArchived to true, and records who archived it.
	/// Captures ArchivedBy (user identifier) and ArchivedAt (timestamp).
	/// </summary>
	Task<bool> ArchiveAsync(string issueId, string archivedBy, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of issues in the database.
	/// </summary>
	Task<long> CountAsync(CancellationToken cancellationToken = default);
}
