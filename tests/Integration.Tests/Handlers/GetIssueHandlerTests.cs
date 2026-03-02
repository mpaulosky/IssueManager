namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetIssueHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class GetIssueHandlerTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
			.Build();

	private IIssueRepository _repository = null!;
	private GetIssueHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new IssueRepository(connectionString, TestDatabase);
		_handler = new GetIssueHandler(_repository);
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
	public async Task Handle_ExistingIssueId_ReturnsIssue()
	{
		// Arrange
		var issue = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		var query = new GetIssueQuery(created.Value.Id.ToString());

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(created.Value.Id);
		result.Title.Should().Be("Test Issue");
		result.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistingIssueId_ReturnsNull()
	{
		// Arrange
		var query = new GetIssueQuery(ObjectId.GenerateNewId().ToString());

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetIssueQuery("");

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task HandleGetAll_MultipleIssues_ReturnsAllIssues()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("Issue 1", "Description 1");
		var issue2 = CreateTestIssueDto("Issue 2", "Description 2");
		var issue3 = CreateTestIssueDto("Issue 3", "Description 3");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleGetAll(TestContext.Current.CancellationToken);

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
		var result = await _handler.HandleGetAll(TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}
}
