// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for CreateIssueHandler with a real MongoDB database.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class CreateIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly CreateIssueHandler _handler;

	public CreateIssueHandlerTests(MongoDbFixture fixture)
	{
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		var currentUserService = Substitute.For<ICurrentUserService>();
		currentUserService.UserId.Returns("test-user-id");
		currentUserService.Name.Returns("Test User");
		currentUserService.Email.Returns("test@example.com");
		currentUserService.IsAuthenticated.Returns(true);
		_handler = new CreateIssueHandler(_repository, new CreateIssueValidator(), currentUserService);
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
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Title.Should().Be("Test Issue");
		result.Description.Should().Be("This is a test issue description.");

		// Verify persistence
		var retrievedResult = await _repository.GetByIdAsync(result.Id, TestContext.Current.CancellationToken);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved?.Title.Should().Be("Test Issue");
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
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, TestContext.Current.CancellationToken));
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
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, TestContext.Current.CancellationToken));
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
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task Handle_MultipleIssues_AllPersistedCorrectly()
	{
		// Arrange & Act
		await _handler.Handle(new CreateIssueCommand { Title = "First Issue" }, TestContext.Current.CancellationToken);
		await _handler.Handle(new CreateIssueCommand { Title = "Second Issue" }, TestContext.Current.CancellationToken);
		await _handler.Handle(new CreateIssueCommand { Title = "Third Issue" }, TestContext.Current.CancellationToken);

		// Assert
		var count = await _repository.CountAsync(TestContext.Current.CancellationToken);
		count.Success.Should().BeTrue();
		count.Value.Should().Be(3);

		var allIssuesResult = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
		allIssuesResult.Should().NotBeNull();
		var allIssues = allIssuesResult.Value;
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
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Description.Should().BeEmpty();

		// Verify persistence
		var retrievedResult = await _repository.GetByIdAsync(result.Id, TestContext.Current.CancellationToken);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved?.Description.Should().BeEmpty();
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
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);
		var afterCreation = DateTime.UtcNow.AddSeconds(1);

		// Assert
		result.DateCreated.Should().BeAfter(beforeCreation);
		result.DateCreated.Should().BeBefore(afterCreation);
	}
}
