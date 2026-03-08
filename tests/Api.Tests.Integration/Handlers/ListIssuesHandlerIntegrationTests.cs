// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListIssuesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for ListIssuesHandler with pagination and real MongoDB database.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class ListIssuesHandlerIntegrationTests
{
	private readonly IIssueRepository _repository;
	private readonly ListIssuesHandler _handler;

	public ListIssuesHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new ListIssuesHandler(_repository, new ListIssuesQueryValidator());
	}

	private static IssueDto CreateTestIssueDto(string title, string description, DateTime? dateCreated = null) =>
	new(ObjectId.GenerateNewId(), title, description, dateCreated ?? DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);

	[Fact]
	public async Task Handle_WithPagination_ReturnsCorrectPage()
	{
		// Arrange - Create 50 issues
		for (int i = 0; i < 50; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.Total.Should().Be(50);
		result.TotalPages.Should().Be(3); // 50 / 20 = 2.5 → 3 pages
	}

	[Fact]
	public async Task Handle_SecondPage_ReturnsNextSetOfItems()
	{
		// Arrange - Create 50 issues
		for (int i = 0; i < 50; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 2, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(2);
		result.Total.Should().Be(50);
	}

	[Fact]
	public async Task Handle_ExcludesArchivedIssues()
	{
		// Arrange - Create 10 issues, archive 3
		var issuesToArchive = new List<string>();
		for (int i = 0; i < 10; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
			if (i < 3)
				issuesToArchive.Add(created.Value!.Id.ToString());
		}

		foreach (var id in issuesToArchive)
		{
			await _repository.ArchiveAsync(ObjectId.Parse(id), TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Total.Should().Be(7); // 10 - 3 archived = 7
		result.Items.Should().HaveCount(7);
	}

	[Fact]
	public async Task Handle_EmptyDatabase_ReturnsEmptyList()
	{
		// Arrange - No issues in database
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Items.Should().BeEmpty();
		result.Total.Should().Be(0);
		result.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task Handle_LastPagePartial_ReturnsRemainingItems()
	{
		// Arrange - Create 42 issues
		for (int i = 0; i < 42; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 3, PageSize = 20 };

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Items.Should().HaveCount(2); // 42 - 40 = 2 on the last page
		result.Page.Should().Be(3);
		result.Total.Should().Be(42);
		result.TotalPages.Should().Be(3);
	}

	[Fact]
	[Trait("Category", "slow")]
	public async Task Handle_LargeDataset_PerformanceUnder1Second()
	{
		// Arrange - Create 100 issues
		for (int i = 0; i < 100; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act
		var stopwatch = System.Diagnostics.Stopwatch.StartNew();
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);
		stopwatch.Stop();

		// Assert
		result.Items.Should().HaveCount(20);
		result.Total.Should().Be(100);
		stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 second
	}

	[Fact]
	public async Task Handle_ConcurrentCreates_ReturnsConsistentResults()
	{
		// Arrange - Create 20 issues
		for (int i = 0; i < 20; i++)
		{
			var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
			await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		}

		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		// Act - List while creating a new issue
		var listTask = _handler.Handle(query, TestContext.Current.CancellationToken);

		var newIssue = CreateTestIssueDto("Concurrent Issue", "Created during list");
		var createTask = _repository.CreateAsync(newIssue, TestContext.Current.CancellationToken);

		await Task.WhenAll(listTask, createTask);

		var result = await listTask;

		// Assert - List should be consistent (snapshot isolation)
		result.Items.Should().HaveCount(20);
		result.Total.Should().BeOneOf(20, 21); // Either before or after concurrently create
	}
}
