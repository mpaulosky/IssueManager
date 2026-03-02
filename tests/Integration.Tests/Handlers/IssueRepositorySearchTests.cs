// Copyright (c) 2026. All rights reserved.

namespace Integration.Handlers;

/// <summary>
/// Integration tests for IssueRepository search and filter functionality with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class IssueRepositorySearchTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IIssueRepository _repository = null!;
	private ListIssuesHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new IssueRepository(connectionString, TestDatabase);
		_handler = new ListIssuesHandler(_repository, new ListIssuesQueryValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static IssueDto CreateTestIssueDto(string title, string description, string authorName = "Test User") =>
		new(
			ObjectId.GenerateNewId(),
			title,
			description,
			DateTime.UtcNow,
			null,
			new UserDto(ObjectId.GenerateNewId().ToString(), authorName, "test@example.com"),
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

	[Fact]
	public async Task GetAllAsync_WithSearchTerm_ReturnsMatchingIssues()
	{
		// Arrange - Create issues with different titles/descriptions
		var issue1 = CreateTestIssueDto("Bug in login feature", "Users cannot login");
		var issue2 = CreateTestIssueDto("Feature request for search", "Add search functionality");
		var issue3 = CreateTestIssueDto("Another bug found", "Bug in payment processing");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, SearchTerm = "bug" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Total.Should().Be(2);
		result.Items.Should().Contain(i => i.Title.Contains("Bug", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public async Task GetAllAsync_WithAuthorName_ReturnsMatchingIssues()
	{
		// Arrange - Create issues with different authors
		var issue1 = CreateTestIssueDto("Issue by Alice", "Description 1", "Alice");
		var issue2 = CreateTestIssueDto("Issue by Bob", "Description 2", "Bob");
		var issue3 = CreateTestIssueDto("Another by Alice", "Description 3", "Alice");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, AuthorName = "Alice" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Total.Should().Be(2);
		result.Items.Should().OnlyContain(i => i.Author.Name == "Alice");
	}

	[Fact]
	public async Task GetAllAsync_WithNoFilters_ReturnsAllNonArchivedIssues()
	{
		// Arrange - Create mix of issues and archive some
		var issue1 = CreateTestIssueDto("Issue 1", "Description 1");
		var issue2 = CreateTestIssueDto("Issue 2", "Description 2");
		var issue3 = CreateTestIssueDto("Issue 3", "Description 3");

		var created1 = await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		var created2 = await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		var created3 = await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		// Archive one issue
		await _repository.ArchiveAsync(ObjectId.Parse(created1.Value.Id.ToString()), TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Total.Should().Be(2);
		result.Items.Should().NotContain(i => i.Archived);
	}

	[Fact]
	public async Task GetAllAsync_WithSearchTermAndAuthorName_ReturnsIntersection()
	{
		// Arrange - Create issues with various combinations
		var issue1 = CreateTestIssueDto("Bug in login", "Description 1", "Alice");
		var issue2 = CreateTestIssueDto("Bug in payment", "Description 2", "Bob");
		var issue3 = CreateTestIssueDto("Feature request", "Description 3", "Alice");
		var issue4 = CreateTestIssueDto("Another bug", "Description 4", "Alice");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue4, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery
		{
			Page = 1,
			PageSize = 20,
			SearchTerm = "bug",
			AuthorName = "Alice"
		};

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Total.Should().Be(2);
		result.Items.Should().OnlyContain(i => i.Author.Name == "Alice");
		result.Items.Should().OnlyContain(i => i.Title.Contains("bug", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public async Task GetAllAsync_WithSearchTermCaseInsensitive_ReturnsMatches()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("BUG in system", "Description");
		var issue2 = CreateTestIssueDto("bug found", "Description");
		var issue3 = CreateTestIssueDto("Bug Report", "Description");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, SearchTerm = "BuG" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(3);
		result.Total.Should().Be(3);
	}

	[Fact]
	public async Task GetAllAsync_WithSearchTermInDescription_ReturnsMatches()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("Issue 1", "This contains a critical bug");
		var issue2 = CreateTestIssueDto("Issue 2", "Normal description");
		var issue3 = CreateTestIssueDto("Issue 3", "Another bug description");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue3, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, SearchTerm = "bug" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Total.Should().Be(2);
	}

	[Fact]
	public async Task GetAllAsync_WithSearchTermNoMatches_ReturnsEmpty()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("Feature request", "Add new feature");
		var issue2 = CreateTestIssueDto("Enhancement", "Improve performance");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, SearchTerm = "bug" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.Total.Should().Be(0);
	}

	[Fact]
	public async Task GetAllAsync_WithAuthorNameNoMatches_ReturnsEmpty()
	{
		// Arrange
		var issue1 = CreateTestIssueDto("Issue 1", "Description", "Alice");
		var issue2 = CreateTestIssueDto("Issue 2", "Description", "Bob");

		await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);

		var query = new ListIssuesQuery { Page = 1, PageSize = 20, AuthorName = "Charlie" };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.Total.Should().Be(0);
	}

	[Fact]
	public async Task GetAllAsync_WithFiltersAndPagination_WorksTogether()
	{
		// Arrange - Create 30 issues by Alice containing "bug"
		for (int i = 0; i < 30; i++)
		{
			var issue = CreateTestIssueDto($"Bug #{i + 1}", $"Description {i + 1}", "Alice");
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		// Create some noise (different author)
		for (int i = 0; i < 10; i++)
		{
			var issue = CreateTestIssueDto($"Bug #{i + 1}", $"Description {i + 1}", "Bob");
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery
		{
			Page = 2,
			PageSize = 10,
			SearchTerm = "bug",
			AuthorName = "Alice"
		};

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(10);
		result.Total.Should().Be(30);
		result.Page.Should().Be(2);
		result.TotalPages.Should().Be(3);
		result.Items.Should().OnlyContain(i => i.Author.Name == "Alice");
	}
}
