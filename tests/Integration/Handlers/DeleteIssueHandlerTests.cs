using MongoDB.Bson;
using Shared.DTOs;

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

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty);

	[Fact]
	public async Task ArchiveAsync_ExistingUnarchivedIssue_ReturnsTrue()
	{
		// Arrange
		var issue = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issue);

		// Act
		var result = await _repository.ArchiveAsync(created.Id.ToString());

		// Assert
		result.Should().BeTrue();

		var retrieved = await _repository.GetByIdAsync(created.Id.ToString());
		retrieved!.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_AlreadyArchivedIssue_ReturnsFalse()
	{
		// Arrange
		var issue = CreateTestIssueDto("Already Archived Issue", "Description");
		var created = await _repository.CreateAsync(issue);
		await _repository.ArchiveAsync(created.Id.ToString());

		// Act - archive again (already archived, ModifiedCount = 0)
		var result = await _repository.ArchiveAsync(created.Id.ToString());

		// Assert - should return false since no modification was made
		result.Should().BeFalse();
	}

	[Fact]
	public async Task ArchiveAsync_NonExistentIssue_ReturnsFalse()
	{
		// Act
		var result = await _repository.ArchiveAsync(ObjectId.GenerateNewId().ToString());

		// Assert
		result.Should().BeFalse();
	}
}
