// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteIssueHandler (soft-delete via Archived) with a real MongoDB database.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class DeleteIssueHandlerIntegrationTests
{
	private readonly IIssueRepository _repository;
	private readonly DeleteIssueHandler _handler;

	public DeleteIssueHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new DeleteIssueHandler(_repository, new DeleteIssueValidator());
	}

	private static IssueDto CreateTestIssueDto(string title, string description, bool archived = false) =>
	new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, archived, UserDto.Empty, false, false);

	[Fact]
	public async Task Handle_ValidIssue_SetsArchivedInDatabase()
	{
		// Arrange - Create an issue
		var issue = CreateTestIssueDto("Issue to Delete", "This will be archived");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);

		var command = new DeleteIssueCommand { Id = created.Value.Id };

		// Act
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Verify Archived is set in a database
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		getResult.Should().NotBeNull();
		var dbIssue = getResult.Value;
		dbIssue.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ExcludedFromListByDefault()
	{
		// Arrange - Create and archive an issue
		var issue = CreateTestIssueDto("Issue to Archive", "Test");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);

		var command = new DeleteIssueCommand { Id = created.Value.Id };

		// Act - Archive the issue
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - GetAll (paginated) should exclude archived issues
		var result = await _repository.GetAllAsync(1, 100, cancellationToken: TestContext.Current.CancellationToken);
		var allIssues = result.Value.Items;
		allIssues.Should().NotContain(i => i.Id == created.Value.Id);
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ReturnsNotFoundFailure()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new DeleteIssueCommand { Id = nonExistentId };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_IssueNotDeleted_RecordStillExists()
	{
		// Arrange - Create an issue
		var issue = CreateTestIssueDto("Issue to Archive", "Should still exist in DB");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);

		var command = new DeleteIssueCommand { Id = created.Value.Id };

		// Act - Soft delete
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Record should still exist (soft delete)
		var dbIssue = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		var getResult = dbIssue;
		getResult.Should().NotBeNull();
		var dto = getResult.Value;
		dto.Id.Should().Be(created.Value.Id);
		dto.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_AlreadyArchivedIssue_IsIdempotent()
	{
		// Arrange - Create an already archived issue
		var archivedIssue = CreateTestIssueDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedIssue, TestContext.Current.CancellationToken);

		var command = new DeleteIssueCommand { Id = created.Value.Id };

		// Act - Delete already archived issue (should be idempotent)
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still be archived
		var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbIssueResult.Should().NotBeNull();
		var dbIssue = dbIssueResult.Value;
		dbIssue.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_MultipleIssues_ArchivesOnlySpecifiedIssue()
	{
		// Arrange - Create multiple issues
		var issue1 = CreateTestIssueDto("Issue 1", "To be archived");
		var issue2 = CreateTestIssueDto("Issue 2", "Should remain active");

		var created1 = await _repository.CreateAsync(issue1, TestContext.Current.CancellationToken);
		var created2 = await _repository.CreateAsync(issue2, TestContext.Current.CancellationToken);

		var command = new DeleteIssueCommand { Id = created1.Value.Id };

		// Act
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		var getResult1 = await _repository.GetByIdAsync(created1.Value.Id, TestContext.Current.CancellationToken);
		var getResult2 = await _repository.GetByIdAsync(created2.Value.Id, TestContext.Current.CancellationToken);

		getResult1.Should().NotBeNull();

		var dto = getResult1.Value;

		dto.Archived.Should().BeTrue();
		getResult2.Value.Archived.Should().BeFalse();
	}
}
