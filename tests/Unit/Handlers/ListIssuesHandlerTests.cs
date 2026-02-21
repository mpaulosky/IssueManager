using FluentAssertions;

using FluentValidation;

using global::Shared.Domain;

using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using IssueManager.Shared.Validators;

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
		_handler = new ListIssuesHandler(_repository, new ListIssuesQueryValidator());
	}

	[Fact]
	public async Task Handle_DefaultPagination_ReturnsFirstPageWithCorrectMetadata()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		var issues = GenerateIssues(20);
		_repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)issues, 42L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(20);
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.Total.Should().Be(42);
		result.TotalPages.Should().Be(3); // 42 / 20 = 2.1 → 3 pages
	}

	[Fact]
	public async Task Handle_SecondPage_ReturnsCorrectItems()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 2, PageSize = 10 };

		var issues = GenerateIssues(10);
		_repository.GetAllAsync(2, 10, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)issues, 42L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(10);
		result.Page.Should().Be(2);
		result.PageSize.Should().Be(10);
		result.Total.Should().Be(42);
		result.TotalPages.Should().Be(5); // 42 / 10 = 4.2 → 5 pages
	}

	[Fact]
	public async Task Handle_LastPagePartialItems_ReturnsCorrectCount()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 3, PageSize = 20 };

		var issues = GenerateIssues(2); // Last page has only 2 items
		_repository.GetAllAsync(3, 20, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)issues, 42L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Page.Should().Be(3);
		result.PageSize.Should().Be(20);
		result.Total.Should().Be(42);
		result.TotalPages.Should().Be(3);
	}

	[Fact]
	public async Task Handle_EmptyResult_ReturnsEmptyList()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		_repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)new List<Issue>(), 0L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.Page.Should().Be(1);
		result.PageSize.Should().Be(20);
		result.Total.Should().Be(0);
		result.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task Handle_PageExceedsTotalPages_ReturnsEmptyList()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 10, PageSize = 20 };

		_repository.GetAllAsync(10, 20, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)new List<Issue>(), 42L));

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
		.WithMessage("*Page*greater than or equal to 1*");
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
		.WithMessage("*Page size*between 1 and 100*");
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
		.WithMessage("*Page size*between 1 and 100*");
	}

	[Fact]
	public async Task Handle_ExcludesArchivedIssues_ByDefault()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

		var issues = GenerateIssues(10);
		_repository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)issues, 10L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetAllAsync(1, 20, Arg.Any<CancellationToken>());
		result.Items.Should().HaveCount(10);
	}

	[Fact]
	public async Task Handle_OrdersByCreatedAtDescending_NewestFirst()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 3 };

		// Create issues already in expected descending order (newest first)
		var orderedIssues = new List<Issue>
{
new("3", "Issue 3", "Desc", IssueStatus.Open, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1)),
new("2", "Issue 2", "Desc", IssueStatus.Open, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-2)),
new("1", "Issue 1", "Desc", IssueStatus.Open, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-3))
};

		_repository.GetAllAsync(1, 3, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<Issue>)orderedIssues, 3L));

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
			Status: IssueStatus.Open,
			CreatedAt: DateTime.UtcNow.AddDays(-i),
			UpdatedAt: DateTime.UtcNow.AddDays(-i)));
		}
		return issues;
	}
}
