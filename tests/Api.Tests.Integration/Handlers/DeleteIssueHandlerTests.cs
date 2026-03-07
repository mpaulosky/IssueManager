// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for IssueRepository.ArchiveAsync (soft-delete).
/// Verifies correct behavior when archiving existing, already-archived, and non-existent issues.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class DeleteIssueHandlerTests
{
	private readonly IIssueRepository _repository;

	public DeleteIssueHandlerTests(MongoDbFixture fixture)
	{
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
	}

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);

	[Fact]
	public async Task ArchiveAsync_ExistingUnarchivedIssue_ReturnsTrue()
	{
		// Arrange
		var issue = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.ArchiveAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();

		var retrievedResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_AlreadyArchivedIssue_ReturnsFalse()
	{
		// Arrange
		var issue = CreateTestIssueDto("Already Archived Issue", "Description");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);
		await _repository.ArchiveAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Act - archive again (already archived, ModifiedCount = 0)
		var result = await _repository.ArchiveAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert - should return false since no modification was made
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task ArchiveAsync_NonExistentIssue_ReturnsFalse()
	{
		// Act
		var result = await _repository.ArchiveAsync(ObjectId.GenerateNewId(), TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
