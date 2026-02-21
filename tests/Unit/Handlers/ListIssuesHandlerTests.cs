using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using IssueManager.Shared.Domain.Models;
using NSubstitute;

namespace IssueManager.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for ListIssuesHandler with pagination.
/// </summary>
public class ListIssuesHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly ListIssuesHandler _handler;

	public ListIssuesHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_handler = new ListIssuesHandler(_repository);
	}

	[Fact]
	public async Task Handle_DefaultPagination_ReturnsFirstPageWithCorrectMetadata()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		var issues = GenerateIssues(20);
		_repository.GetAllAsync(1, 20, false, Arg.Any<CancellationToken>())
			.Returns(issues);

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(42);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.TotalCount.Should().Be(42);
		result.TotalPages.Should().Be(3); // 42 / 20 = 2.1 → 3 pages
	}

	[Fact]
	public async Task Handle_SecondPage_ReturnsCorrectItems()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 2, PageSize = 10 };

		var issues = GenerateIssues(10);
		_repository.GetAllAsync(2, 10, false, Arg.Any<CancellationToken>())
			.Returns(issues);

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(42);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(10);
		result.Page.Should().Be(2);
		result.PageSize.Should().Be(10);
		result.TotalCount.Should().Be(42);
		result.TotalPages.Should().Be(5); // 42 / 10 = 4.2 → 5 pages
	}

	[Fact]
	public async Task Handle_LastPagePartialItems_ReturnsCorrectCount()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 3, PageSize = 20 };

		var issues = GenerateIssues(2); // Last page has only 2 items
		_repository.GetAllAsync(3, 20, false, Arg.Any<CancellationToken>())
			.Returns(issues);

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(42);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Page.Should().Be(3);
		result.PageSize.Should().Be(20);
		result.TotalCount.Should().Be(42);
		result.TotalPages.Should().Be(3);
	}

	[Fact]
	public async Task Handle_EmptyResult_ReturnsEmptyList()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		_repository.GetAllAsync(1, 20, false, Arg.Any<CancellationToken>())
			.Returns(new List<Issue>());

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(0);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.TotalCount.Should().Be(0);
		result.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task Handle_PageExceedsTotalPages_ReturnsEmptyList()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 10, PageSize = 20 };

		_repository.GetAllAsync(10, 20, false, Arg.Any<CancellationToken>())
			.Returns(new List<Issue>());

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(42); // Only 3 pages exist

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.Page.Should().Be(10);
		result.TotalPages.Should().Be(3);
	}

	[Fact]
	public async Task Handle_InvalidPage_ThrowsValidationException()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 0, PageSize = 20 };

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Page*greater than 0*");
	}

	[Fact]
	public async Task Handle_InvalidPageSize_ThrowsValidationException()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 0 };

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*PageSize*greater than 0*");
	}

	[Fact]
	public async Task Handle_PageSizeExceedsMax_ThrowsValidationException()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 101 };

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*PageSize*100*");
	}

	[Fact]
	public async Task Handle_ExcludesArchivedIssues_ByDefault()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		var issues = GenerateIssues(10);
		_repository.GetAllAsync(1, 20, false, Arg.Any<CancellationToken>())
			.Returns(issues);

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(10);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetAllAsync(1, 20, false, Arg.Any<CancellationToken>());
		result.Items.Should().HaveCount(10);
	}

	[Fact]
	public async Task Handle_OrdersByCreatedAtDescending_NewestFirst()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 3 };

		var issues = new List<Issue>
		{
			new("1", "Issue 1", "Desc", "user1", DateTime.UtcNow.AddDays(-3)),
			new("2", "Issue 2", "Desc", "user2", DateTime.UtcNow.AddDays(-2)),
			new("3", "Issue 3", "Desc", "user3", DateTime.UtcNow.AddDays(-1))
		};

		_repository.GetAllAsync(1, 3, false, Arg.Any<CancellationToken>())
			.Returns(issues.OrderByDescending(i => i.CreatedAt).ToList());

		_repository.CountAsync(false, Arg.Any<CancellationToken>())
			.Returns(3);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(3);
		result.Items[0].Id.Should().Be("3"); // Newest first
		result.Items[1].Id.Should().Be("2");
		result.Items[2].Id.Should().Be("1"); // Oldest last
	}

	private static List<Issue> GenerateIssues(int count)
	{
		var issues = new List<Issue>();
		for (int i = 0; i < count; i++)
		{
			issues.Add(new Issue(
				Id: Guid.NewGuid().ToString(),
				Title: $"Issue {i + 1}",
				Description: $"Description {i + 1}",
				AuthorId: "user-123",
				CreatedAt: DateTime.UtcNow.AddDays(-i)));
		}
		return issues;
	}
}
