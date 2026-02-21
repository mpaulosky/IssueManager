using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Shared.Domain.Models;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Data;

/// <summary>
/// Integration tests for IssueRepository with pagination, filtering, and soft-delete.
/// </summary>
public class IssueRepositoryTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;

	public IssueRepositoryTests()
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
	public async Task GetAllAsync_FirstPage_ReturnsCorrectItems()
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

		// Act
		var result = await _repository.GetAllAsync(page: 1, pageSize: 20, includeArchived: false);

		// Assert
		result.Should().HaveCount(20);
	}

	[Fact]
	public async Task GetAllAsync_SecondPage_ReturnsNextSetOfItems()
	{
		// Arrange - Create 50 issues
		var issueIds = new List<string>();
		for (int i = 0; i < 50; i++)
		{
			var issue = new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddMinutes(-i));

			await _repository.CreateAsync(issue);
			issueIds.Add(issue.Id);
		}

		// Act
		var page1 = await _repository.GetAllAsync(page: 1, pageSize: 20, includeArchived: false);
		var page2 = await _repository.GetAllAsync(page: 2, pageSize: 20, includeArchived: false);

		// Assert
		page2.Should().HaveCount(20);
		page1.Select(i => i.Id).Should().NotIntersectWith(page2.Select(i => i.Id)); // No overlap
	}

	[Fact]
	public async Task GetAllAsync_ExcludesArchived_ByDefault()
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
				IsArchived = i < 3 // Archive first 3
			};

			await _repository.CreateAsync(issue);
		}

		// Act
		var result = await _repository.GetAllAsync(page: 1, pageSize: 20, includeArchived: false);

		// Assert
		result.Should().HaveCount(7); // 10 - 3 archived = 7
		result.Should().OnlyContain(i => !i.IsArchived);
	}

	[Fact]
	public async Task GetAllAsync_IncludesArchived_WhenRequested()
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
				IsArchived = i < 3
			};

			await _repository.CreateAsync(issue);
		}

		// Act
		var result = await _repository.GetAllAsync(page: 1, pageSize: 20, includeArchived: true);

		// Assert
		result.Should().HaveCount(10); // All issues including archived
	}

	[Fact]
	public async Task CountAsync_ExcludesArchived_ByDefault()
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
				IsArchived = i < 3
			};

			await _repository.CreateAsync(issue);
		}

		// Act
		var count = await _repository.CountAsync(includeArchived: false);

		// Assert
		count.Should().Be(7); // 10 - 3 archived = 7
	}

	[Fact]
	public async Task CountAsync_IncludesArchived_WhenRequested()
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
				IsArchived = i < 3
			};

			await _repository.CreateAsync(issue);
		}

		// Act
		var count = await _repository.CountAsync(includeArchived: true);

		// Assert
		count.Should().Be(10); // All issues
	}

	[Fact]
	public async Task ArchiveAsync_SetsIsArchivedToTrue()
	{
		// Arrange - Create an issue
		var issue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Issue to Archive",
			Description: "Test",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow)
		{
			IsArchived = false
		};

		await _repository.CreateAsync(issue);

		// Act
		var archivedIssue = issue with { IsArchived = true, UpdatedAt = DateTime.UtcNow };
		var result = await _repository.UpdateAsync(archivedIssue);

		// Assert
		result.Should().NotBeNull();
		result!.IsArchived.Should().BeTrue();

		// Verify in database
		var dbIssue = await _repository.GetByIdAsync(issue.Id);
		dbIssue!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_UpdatesTimestamp()
	{
		// Arrange - Create an issue
		var issue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Issue to Archive",
			Description: "Test",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = false,
			UpdatedAt = DateTime.UtcNow.AddHours(-2)
		};

		await _repository.CreateAsync(issue);

		// Act
		var archivedIssue = issue with { IsArchived = true, UpdatedAt = DateTime.UtcNow };
		var result = await _repository.UpdateAsync(archivedIssue);

		// Assert
		result!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
		result.UpdatedAt.Should().BeAfter(issue.UpdatedAt!.Value);
	}

	[Fact]
	public async Task ArchiveAsync_DoesNotDeleteRecord()
	{
		// Arrange - Create an issue
		var issue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Issue to Archive",
			Description: "Should still exist",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		await _repository.CreateAsync(issue);

		// Act - Soft delete (archive)
		var archivedIssue = issue with { IsArchived = true, UpdatedAt = DateTime.UtcNow };
		await _repository.UpdateAsync(archivedIssue);

		// Assert - Record still exists
		var dbIssue = await _repository.GetByIdAsync(issue.Id);
		dbIssue.Should().NotBeNull();
		dbIssue!.Id.Should().Be(issue.Id);
		dbIssue.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task UpdateAsync_NonExistentIssue_ReturnsNull()
	{
		// Arrange
		var nonExistentIssue = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Non-existent",
			Description: "Does not exist",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow);

		// Act
		var result = await _repository.UpdateAsync(nonExistentIssue);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task GetAllAsync_OrdersByCreatedAtDescending()
	{
		// Arrange - Create issues with specific timestamps
		var issue1 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Oldest",
			Description: "Created first",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-3));

		var issue2 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Middle",
			Description: "Created second",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-2));

		var issue3 = new Issue(
			Id: Guid.NewGuid().ToString(),
			Title: "Newest",
			Description: "Created last",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1));

		await _repository.CreateAsync(issue1);
		await _repository.CreateAsync(issue2);
		await _repository.CreateAsync(issue3);

		// Act
		var result = await _repository.GetAllAsync(page: 1, pageSize: 10, includeArchived: false);

		// Assert
		result.Should().HaveCount(3);
		result[0].Title.Should().Be("Newest"); // Newest first
		result[1].Title.Should().Be("Middle");
		result[2].Title.Should().Be("Oldest"); // Oldest last
	}

	[Fact]
	public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
	{
		// Act
		var result = await _repository.GetAllAsync(page: 1, pageSize: 20, includeArchived: false);

		// Assert
		result.Should().BeEmpty();
	}
}
