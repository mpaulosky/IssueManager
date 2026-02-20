using Shared.Domain;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for GetIssueHandler with real MongoDB database.
/// </summary>
public class GetIssueHandlerTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;
	private GetIssueHandler _handler = null!;

	public GetIssueHandlerTests()
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
		_handler = new GetIssueHandler(_repository);
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
	public async Task Handle_ExistingIssueId_ReturnsIssue()
	{
		// Arrange
		var issue = Issue.Create("Test Issue", "Test Description");
		await _repository.CreateAsync(issue);
		var query = new GetIssueQuery(issue.Id);

		// Act
		var result = await _handler.Handle(query);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(issue.Id);
		result.Title.Should().Be("Test Issue");
		result.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistingIssueId_ReturnsNull()
	{
		// Arrange
		var query = new GetIssueQuery("non-existing-id");

		// Act
		var result = await _handler.Handle(query);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetIssueQuery("");

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query));
	}

	[Fact]
	public async Task HandleGetAll_MultipleIssues_ReturnsAllIssues()
	{
		// Arrange
		var issue1 = Issue.Create("Issue 1", "Description 1");
		var issue2 = Issue.Create("Issue 2", "Description 2");
		var issue3 = Issue.Create("Issue 3", "Description 3");

		await _repository.CreateAsync(issue1);
		await _repository.CreateAsync(issue2);
		await _repository.CreateAsync(issue3);

		// Act
		var result = await _handler.HandleGetAll();

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(i => i.Title == "Issue 1");
		result.Should().Contain(i => i.Title == "Issue 2");
		result.Should().Contain(i => i.Title == "Issue 3");
	}

	[Fact]
	public async Task HandleGetAll_EmptyDatabase_ReturnsEmptyList()
	{
		// Act
		var result = await _handler.HandleGetAll();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}
