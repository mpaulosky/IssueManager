using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using global::Shared.Domain;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for ListIssuesHandler with pagination and real MongoDB database.
/// </summary>
public class ListIssuesHandlerIntegrationTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;
	private ListIssuesHandler _handler = null!;

	public ListIssuesHandlerIntegrationTests()
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
		_handler = new ListIssuesHandler(_repository);
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
	public async Task Handle_WithPagination_ReturnsCorrectPage()
	{
		// Arrange - Create 50 issues
		for (int i = 0; i < 50; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.TotalCount.Should().Be(50);
		result.TotalPages.Should().Be(3); // 50 / 20 = 2.5 → 3 pages
	}

	[Fact]
	public async Task Handle_SecondPage_ReturnsNextSetOfItems()
	{
		// Arrange - Create 50 issues
		for (int i = 0; i < 50; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 2, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(2);
		result.TotalCount.Should().Be(50);
	}

	[Fact]
	public async Task Handle_ExcludesArchivedIssues()
	{
		// Arrange - Create 10 issues, archive 3
		for (int i = 0; i < 10; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i))
			{
				IsArchived = i < 3 // Archive first 3 issues
			};

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.TotalCount.Should().Be(7); // 10 - 3 archived = 7
		result.Items.Should().HaveCount(7);
		result.Items.Should().OnlyContain(i => !i.IsArchived);
	}

	[Fact]
	public async Task Handle_OrdersByCreatedAtDescending()
	{
		// Arrange - Create issues with specific timestamps
		var issue1 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Oldest Issue",
			Description: "Created first",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-3));

		var issue2 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Middle Issue",
			Description: "Created second",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-2));

		var issue3 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Newest Issue",
			Description: "Created last",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1));

		await _repository.CreateAsync(issue1);
		await _repository.CreateAsync(issue2);
		await _repository.CreateAsync(issue3);

		var query = new ListIssuesQuery { Page = 1, PageSize = 10 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(3);
		result.Items[0].Title.Should().Be("Newest Issue"); // Newest first
		result.Items[1].Title.Should().Be("Middle Issue");
		result.Items[2].Title.Should().Be("Oldest Issue"); // Oldest last
	}

	[Fact]
	public async Task Handle_EmptyDatabase_ReturnsEmptyList()
	{
		// Arrange - No issues in database
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
		result.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task Handle_LastPagePartial_ReturnsRemainingItems()
	{
		// Arrange - Create 42 issues
		for (int i = 0; i < 42; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 3, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2); // 42 - 40 = 2 on last page
		result.Page.Should().Be(3);
		result.TotalCount.Should().Be(42);
		result.TotalPages.Should().Be(3);
	}

	[Fact]
	public async Task Handle_LargeDataset_PerformanceUnder1Second()
	{
		// Arrange - Create 1000 issues
		for (int i = 0; i < 1000; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var stopwatch = System.Diagnostics.Stopwatch.StartNew();
		var result = await _handler.Handle(query, CancellationToken.None);
		stopwatch.Stop();

		// Assert
		result.Items.Should().HaveCount(20);
		result.TotalCount.Should().Be(1000);
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 second
	}

	[Fact]
	public async Task Handle_ConcurrentCreates_ReturnsConsistentResults()
	{
		// Arrange - Create 20 issues
		for (int i = 0; i < 20; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act - List while creating new issue
		var listTask = _handler.Handle(query, CancellationToken.None);

		var newIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Concurrent Issue",
			Description: "Created during list",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		var createTask = _repository.CreateAsync(newIssue);

		await Task.WhenAll(listTask, createTask);

		var result = await listTask;

		// Assert - List should be consistent (snapshot isolation)
		result.Items.Should().HaveCount(20);
		result.TotalCount.Should().BeOneOf(20, 21); // Either before or after concurrent create
	}
}
