// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListIssuesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================
namespace Api.Handlers.Issues;

/// <summary>
/// Unit tests for ListIssuesHandler with pagination.
/// </summary>
[ExcludeFromCodeCoverage]
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

		var issues = GenerateIssueDtos(20);
		_repository.GetAllAsync(1, 20, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)issues, 42L));

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

		var issues = GenerateIssueDtos(10);
		_repository.GetAllAsync(2, 10, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)issues, 42L));

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

		var issues = GenerateIssueDtos(2); // Last page has only 2 items
		_repository.GetAllAsync(3, 20, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)issues, 42L));

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

		_repository.GetAllAsync(1, 20, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)new List<IssueDto>(), 0L));

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

		_repository.GetAllAsync(10, 20, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)new List<IssueDto>(), 42L));

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

		var issues = GenerateIssueDtos(10);
		_repository.GetAllAsync(1, 20, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)issues, 10L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetAllAsync(1, 20, null, null, Arg.Any<CancellationToken>());
		result.Items.Should().HaveCount(10);
	}

	[Fact]
	public async Task Handle_OrdersByCreatedAtDescending_NewestFirst()
	{
		// Arrange
		var query = new ListIssuesQuery { Page = 1, PageSize = 3 };

		// Create issues already in expected descending order (newest first)
		var orderedIssues = new List<IssueDto>
		{
			new(ObjectId.GenerateNewId(), "Issue 3", "Desc", DateTime.UtcNow.AddDays(-1), null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false),
			new(ObjectId.GenerateNewId(), "Issue 2", "Desc", DateTime.UtcNow.AddDays(-2), null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false),
			new(ObjectId.GenerateNewId(), "Issue 1", "Desc", DateTime.UtcNow.AddDays(-3), null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false),
		};

		_repository.GetAllAsync(1, 3, null, null, Arg.Any<CancellationToken>())
		.Returns(((IReadOnlyList<IssueDto>)orderedIssues, 3L));

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(3);
		result.Items[0].Title.Should().Be("Issue 3"); // Newest first
		result.Items[1].Title.Should().Be("Issue 2");
		result.Items[2].Title.Should().Be("Issue 1"); // Oldest last
	}

	private static List<IssueDto> GenerateIssueDtos(int count)
	{
		var issues = new List<IssueDto>();
		for (int i = 0; i < count; i++)
		{
			issues.Add(new IssueDto(
				ObjectId.GenerateNewId(),
				$"Issue {i + 1}",
				$"Description {i + 1}",
				DateTime.UtcNow.AddDays(-i),
				null,
				UserDto.Empty,
				CategoryDto.Empty,
				new StatusDto(ObjectId.Empty, "Open", "Issue is open", DateTime.UtcNow, null, false, UserDto.Empty),
				false,
				UserDto.Empty,
				false,
				false));
		}
		return issues;
	}
}
