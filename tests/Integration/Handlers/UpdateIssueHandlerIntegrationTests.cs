using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using global::Shared.Domain;
using IssueManager.Shared.Validators;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueHandler with real MongoDB database.
/// </summary>
public class UpdateIssueHandlerIntegrationTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;
	private UpdateIssueHandler _handler = null!;

	public UpdateIssueHandlerIntegrationTests()
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
		_handler = new UpdateIssueHandler(_repository, new UpdateIssueValidator());
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
	public async Task Handle_ValidUpdate_UpdatesIssueInDatabase()
	{
		// Arrange - Create an issue first
		var originalIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		await _repository.CreateAsync(originalIssue);

		var command = new UpdateIssueCommand
		{
			Id = originalIssue.Id,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(originalIssue.Id);
		result.Title.Should().Be("Updated Title");
		result.Description.Should().Be("Updated Description");
		result.UpdatedAt.Should().NotBeNull();
		result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

		// Verify in database
		var dbIssue = await _repository.GetByIdAsync(originalIssue.Id);
		dbIssue.Should().NotBeNull();
		dbIssue!.Title.Should().Be("Updated Title");
		dbIssue.Description.Should().Be("Updated Description");
		dbIssue.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task Handle_UpdateTimestamp_SetsToCurrentTime()
	{
		// Arrange
		var originalIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			UpdatedAt = DateTime.UtcNow.AddHours(-5)
		};

		await _repository.CreateAsync(originalIssue);

		var command = new UpdateIssueCommand
		{
			Id = originalIssue.Id,
			Title = "New Title",
			Description = "New Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.UpdatedAt.Should().NotBeNull();
		result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
		result.UpdatedAt.Should().BeAfter(originalIssue.UpdatedAt!.Value);

		// Verify in database
		var dbIssue = await _repository.GetByIdAsync(originalIssue.Id);
		dbIssue!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Handle_AtomicUpdate_TitleAndDescriptionBothUpdate()
	{
		// Arrange
		var originalIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		await _repository.CreateAsync(originalIssue);

		var command = new UpdateIssueCommand
		{
			Id = originalIssue.Id,
			Title = "New Title",
			Description = "New Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert - Both fields should be updated atomically
		var dbIssue = await _repository.GetByIdAsync(originalIssue.Id);
		dbIssue.Should().NotBeNull();
		dbIssue!.Title.Should().Be("New Title");
		dbIssue.Description.Should().Be("New Description");
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid().ToString();
		var command = new UpdateIssueCommand
		{
			Id = nonExistentId,
			Title = "Title",
			Description = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_ConcurrentUpdates_LastWriteWins()
	{
		// Arrange - Create an issue
		var issue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		await _repository.CreateAsync(issue);

		var command1 = new UpdateIssueCommand
		{
			Id = issue.Id,
			Title = "First Update",
			Description = "First Description"
		};

		var command2 = new UpdateIssueCommand
		{
			Id = issue.Id,
			Title = "Second Update",
			Description = "Second Description"
		};

		// Act - Simulate concurrent updates
		var result1 = await _handler.Handle(command1, CancellationToken.None);
		await Task.Delay(100); // Small delay to ensure different timestamp
		var result2 = await _handler.Handle(command2, CancellationToken.None);

		// Assert - Last write wins
		var dbIssue = await _repository.GetByIdAsync(issue.Id);
		dbIssue.Should().NotBeNull();
		dbIssue!.Title.Should().Be("Second Update");
		dbIssue.Description.Should().Be("Second Description");
		dbIssue.UpdatedAt.Should().BeAfter(result1.UpdatedAt!.Value);
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ThrowsConflictException()
	{
		// Arrange - Create and archive an issue
		var archivedIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Archived Issue",
			Description: "This is archived",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow)
		{
			IsArchived = true
		};

		await _repository.CreateAsync(archivedIssue);

		var command = new UpdateIssueCommand
		{
			Id = archivedIssue.Id,
			Title = "Attempt Update",
			Description = "Should fail"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ConflictException>();

		// Verify issue wasn't updated
		var dbIssue = await _repository.GetByIdAsync(archivedIssue.Id);
		dbIssue!.Title.Should().Be("Archived Issue");
	}
}
