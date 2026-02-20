using IssueManager.Tests.Integration.Fixtures;

using Shared.Domain;
using Shared.Validators;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for CreateIssueHandler with real MongoDB database.
/// </summary>
public class CreateIssueHandlerTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;
	private CreateIssueHandler _handler = null!;

	public CreateIssueHandlerTests()
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
		_handler = new CreateIssueHandler(_repository, new CreateIssueValidator());
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
	public async Task Handle_ValidCommand_StoresIssueInDatabase()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Test Issue",
			Description = "This is a test issue description."
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
		result.Title.Should().Be("Test Issue");
		result.Description.Should().Be("This is a test issue description.");
		result.Status.Should().Be(IssueStatus.Open);

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(result.Id);
		retrieved.Should().NotBeNull();
		retrieved!.Title.Should().Be("Test Issue");
	}

	[Fact]
	public async Task Handle_ValidCommandWithLabels_StoresIssueWithLabels()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Bug Report",
			Description = "Found a critical bug",
			Labels = new List<string> { "bug", "critical", "backend" }
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result.Labels.Should().HaveCount(3);
		result.Labels.Should().Contain(l => l.Name == "bug");
		result.Labels.Should().Contain(l => l.Name == "critical");
		result.Labels.Should().Contain(l => l.Name == "backend");

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(result.Id);
		retrieved!.Labels.Should().HaveCount(3);
	}

	[Fact]
	public async Task Handle_EmptyTitle_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "",
			Description = "Description without title"
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
	}

	[Fact]
	public async Task Handle_TitleTooShort_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "AB",
			Description = "Title is too short"
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
	}

	[Fact]
	public async Task Handle_TitleTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = new string('X', 201),
			Description = "Title exceeds maximum length"
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
	}

	[Fact]
	public async Task Handle_MultipleIssues_AllPersistedCorrectly()
	{
		// Arrange & Act
		var issue1 = await _handler.Handle(new CreateIssueCommand { Title = "First Issue" });
		var issue2 = await _handler.Handle(new CreateIssueCommand { Title = "Second Issue" });
		var issue3 = await _handler.Handle(new CreateIssueCommand { Title = "Third Issue" });

		// Assert
		var count = await _repository.CountAsync();
		count.Should().Be(3);

		var allIssues = await _repository.GetAllAsync();
		allIssues.Should().HaveCount(3);
		allIssues.Should().Contain(i => i.Title == "First Issue");
		allIssues.Should().Contain(i => i.Title == "Second Issue");
		allIssues.Should().Contain(i => i.Title == "Third Issue");
	}

	[Fact]
	public async Task Handle_ValidCommandWithNullDescription_CreatesIssue()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Issue without description",
			Description = null
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result.Description.Should().BeNull();

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(result.Id);
		retrieved!.Description.Should().BeNull();
	}

	[Fact]
	public async Task Handle_CreatedIssue_HasCorrectTimestamps()
	{
		// Arrange
		var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
		var command = new CreateIssueCommand
		{
			Title = "Timestamp Test Issue"
		};

		// Act
		var result = await _handler.Handle(command);
		var afterCreation = DateTime.UtcNow.AddSeconds(1);

		// Assert
		result.CreatedAt.Should().BeAfter(beforeCreation);
		result.CreatedAt.Should().BeBefore(afterCreation);
		result.UpdatedAt.Should().BeAfter(beforeCreation);
		result.UpdatedAt.Should().BeBefore(afterCreation);
		result.CreatedAt.Should().BeCloseTo(result.UpdatedAt, TimeSpan.FromSeconds(1));
	}
}
