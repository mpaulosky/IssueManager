using Tests.Integration.Fixtures;

using Shared.DTOs;
using Shared.Validators;

namespace Tests.Integration.Handlers;

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
		result.Id.Should().NotBe(MongoDB.Bson.ObjectId.Empty);
		result.Title.Should().Be("Test Issue");
		result.Description.Should().Be("This is a test issue description.");

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(result.Id.ToString());
		retrieved.Should().NotBeNull();
		retrieved!.Title.Should().Be("Test Issue");
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
		result.Description.Should().BeEmpty();

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(result.Id.ToString());
		retrieved!.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_CreatedIssue_HasDateCreated()
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
		result.DateCreated.Should().BeAfter(beforeCreation);
		result.DateCreated.Should().BeBefore(afterCreation);
	}
}
