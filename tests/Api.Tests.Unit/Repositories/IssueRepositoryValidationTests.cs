// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueRepositoryValidationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =============================================

namespace Api.Repositories;

/// <summary>
/// Unit tests for IssueRepository validation logic that can be tested without a database.
/// These tests validate input validation (ObjectId.Empty checks) before any database operations occur.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class IssueRepositoryValidationTests
{
	// ========== IssueRepository Validation Tests ==========

	[Fact]
	public async Task IssueRepository_ArchiveAsync_WithEmptyObjectId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.ArchiveAsync(ObjectId.Empty, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Issue ID cannot be empty.");
	}

	[Fact]
	public async Task IssueRepository_CreateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");
		var issueWithEmptyId = IssueDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.CreateAsync(issueWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Issue ID cannot be empty.");
	}

	[Fact]
	public async Task IssueRepository_UpdateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");
		var issueWithEmptyId = IssueDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.UpdateAsync(issueWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Issue ID cannot be empty.");
	}
}
