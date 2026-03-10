// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentRepositoryValidationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =============================================

namespace Api.Repositories;

/// <summary>
/// Unit tests for CommentRepository validation logic that can be tested without a database.
/// These tests validate input validation (ObjectId.Empty checks and string validation) before any database operations occur.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CommentRepositoryValidationTests
{
	// ========== CommentRepository Validation Tests ==========

	[Fact]
	public async Task CommentRepository_GetByUserAsync_WithEmptyUserId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		const string emptyUserId = "";

		// Act
		var result = await repo.GetByUserAsync(emptyUserId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("User ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_GetByUserAsync_WithWhitespaceUserId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		const string whitespaceUserId = "   ";

		// Act
		var result = await repo.GetByUserAsync(whitespaceUserId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("User ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_GetByUserAsync_WithNullUserId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		string? nullUserId = null;

		// Act
		var result = await repo.GetByUserAsync(nullUserId!, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("User ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_GetByIssueAsync_WithEmptyIssueId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		var issueWithEmptyId = IssueDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.GetByIssueAsync(issueWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Issue ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_UpVoteAsync_WithEmptyUserId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		var commentId = ObjectId.GenerateNewId();
		const string emptyUserId = "";

		// Act
		var result = await repo.UpVoteAsync(commentId, emptyUserId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("User ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_UpVoteAsync_WithWhitespaceUserId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		var commentId = ObjectId.GenerateNewId();
		const string whitespaceUserId = "  ";

		// Act
		var result = await repo.UpVoteAsync(commentId, whitespaceUserId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("User ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_CreateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		var commentWithEmptyId = CommentDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.CreateAsync(commentWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Comment ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_ArchiveAsync_WithEmptyObjectId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.ArchiveAsync(ObjectId.Empty, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Comment ID cannot be empty.");
	}

	[Fact]
	public async Task CommentRepository_UpdateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
		var commentWithEmptyId = CommentDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.UpdateAsync(commentWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Comment ID cannot be empty.");
	}
}
