using Shared.Domain;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for IssueRepository.ArchiveAsync (soft-delete).
/// Verifies correct behavior when archiving existing, already-archived, and non-existent issues.
/// </summary>
public class DeleteIssueHandlerTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;

	public DeleteIssueHandlerTests()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithImage(MONGODB_IMAGE)
			.Build();
	}

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async Task InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new IssueRepository(connectionString, TEST_DATABASE);
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async Task DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	[Fact]
	public async Task ArchiveAsync_ExistingUnarchivedIssue_ReturnsTrue()
	{
		// Arrange
		var issue = Issue.Create("Test Issue", "Test Description");
		await _repository.CreateAsync(issue);

		// Act
		var result = await _repository.ArchiveAsync(issue.Id);

		// Assert
		result.Should().BeTrue();

		var retrieved = await _repository.GetByIdAsync(issue.Id);
		retrieved!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_AlreadyArchivedIssue_ReturnsTrueIdempotent()
	{
		// Arrange
		var issue = Issue.Create("Already Archived Issue", "Description");
		await _repository.CreateAsync(issue);
		await _repository.ArchiveAsync(issue.Id);

		// Act - archive again (already archived, MatchedCount still > 0)
		var result = await _repository.ArchiveAsync(issue.Id);

		// Assert - should return true (issue was found), not false (issue not found)
		result.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_NonExistentIssue_ReturnsFalse()
	{
		// Act
		var result = await _repository.ArchiveAsync("non-existent-id");

		// Assert
		result.Should().BeFalse();
	}
}
