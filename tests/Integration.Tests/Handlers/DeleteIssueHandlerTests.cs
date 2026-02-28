namespace Integration.Handlers;

/// <summary>
/// Integration tests for IssueRepository.ArchiveAsync (soft-delete).
/// Verifies correct behavior when archiving existing, already-archived, and non-existent issues.
/// </summary>
[Collection("Integration")]
public class DeleteIssueHandlerTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:8.2";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
			.WithImage(MongodbImage)
			.Build();

	private IIssueRepository _repository = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new IssueRepository(connectionString, TestDatabase);
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);

	[Fact]
	public async Task ArchiveAsync_ExistingUnarchivedIssue_ReturnsTrue()
	{
		// Arrange
		var issue = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issue);

		// Act
		var result = await _repository.ArchiveAsync(created.Value.Id);

		// Assert
		result.Success.Should().BeTrue();

		var retrievedResult = await _repository.GetByIdAsync(created.Value.Id);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_AlreadyArchivedIssue_ReturnsFalse()
	{
		// Arrange
		var issue = CreateTestIssueDto("Already Archived Issue", "Description");
		var created = await _repository.CreateAsync(issue);
		await _repository.ArchiveAsync(created.Value.Id);

		// Act - archive again (already archived, ModifiedCount = 0)
		var result = await _repository.ArchiveAsync(created.Value.Id);

		// Assert - should return false since no modification was made
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task ArchiveAsync_NonExistentIssue_ReturnsFalse()
	{
		// Act
		var result = await _repository.ArchiveAsync(ObjectId.GenerateNewId());

		// Assert
		result.Success.Should().BeFalse();
	}
}
