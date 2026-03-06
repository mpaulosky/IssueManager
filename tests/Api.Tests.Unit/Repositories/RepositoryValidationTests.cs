// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     RepositoryValidationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Unit.Repositories;

/// <summary>
/// Unit tests for repository validation logic that can be tested without a database.
/// These tests validate input validation and ObjectId parsing logic.
/// Note: Full CRUD operations are covered by integration tests.
/// </summary>
public sealed class RepositoryValidationTests
{
	// ========== IssueRepository ObjectId Validation Tests ==========
	// NOTE: These tests are commented out because the repository no longer accepts string IDs.
	// Validation is now handled at the handler level before calling repository methods.

	// [Fact]
	// public async Task IssueRepository_GetByIdAsync_WithInvalidObjectId_ReturnsNull()
	// {
	// 	// Arrange
	// 	var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");
	// 	const string invalidId = "invalid-id";
	// 
	// 	// Act
	// 	var result = await repo.GetByIdAsync(invalidId);
	// 
	// 	// Assert
	// 	result.Should().BeNull("invalid ObjectId should return null without DB call");
	// }

	// [Fact]
	// public async Task IssueRepository_DeleteAsync_WithInvalidObjectId_ReturnsFalse()
	// {
	// 	// Arrange
	// 	var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");
	// 	const string invalidId = "not-a-valid-objectid";
	// 
	// 	// Act
	// 	var result = await repo.DeleteAsync(invalidId);
	// 
	// 	// Assert
	// 	result.Should().BeFalse("invalid ObjectId should return false without DB call");
	// }

	// [Fact]
	// public async Task IssueRepository_ArchiveAsync_WithInvalidObjectId_ReturnsFalse()
	// {
	// 	// Arrange
	// 	var repo = new IssueRepository("mongodb://localhost:27017", "TestDb");
	// 	const string invalidId = "12345";
	// 
	// 	// Act
	// 	var result = await repo.ArchiveAsync(invalidId);
	// 
	// 	// Assert
	// 	result.Should().BeFalse("invalid ObjectId should return false without DB call");
	// }

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
	public async Task CommentRepository_GetByIssueAsync_WithNullIssue_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.GetByIssueAsync(null!, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Issue cannot be null.");
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

	// ========== CategoryRepository Null Validation Tests ==========
	// NOTE: These tests are commented out because repository signatures have changed.
	// ArchiveAsync now takes ObjectId only, UpdateAsync takes CategoryDto only.

	[Fact]
	public async Task CategoryRepository_CreateAsync_WithNullCategory_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.CreateAsync(null!, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category cannot be null.");
	}

	// [Fact]
	// public async Task CategoryRepository_ArchiveAsync_WithNullCategory_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");
	// 
	// 	// Act
	// 	var result = await repo.ArchiveAsync(null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Category cannot be null.");
	// }

	// [Fact]
	// public async Task CategoryRepository_UpdateAsync_WithNullCategory_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");
	// 	var categoryId = ObjectId.GenerateNewId();
	// 
	// 	// Act
	// 	var result = await repo.UpdateAsync(categoryId, null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Category cannot be null.");
	// }

	// ========== StatusRepository Null Validation Tests ==========
	// NOTE: These tests are commented out because repository signatures have changed.
	// ArchiveAsync now takes ObjectId only, UpdateAsync takes StatusDto only.

	[Fact]
	public async Task StatusRepository_CreateAsync_WithNullStatus_ReturnsFailureResult()
	{
		// Arrange
		var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.CreateAsync(null!, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Status cannot be null.");
	}

	// [Fact]
	// public async Task StatusRepository_ArchiveAsync_WithNullStatus_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");
	// 
	// 	// Act
	// 	var result = await repo.ArchiveAsync(null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Status cannot be null.");
	// }

	// [Fact]
	// public async Task StatusRepository_UpdateAsync_WithNullStatus_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");
	// 	var statusId = ObjectId.GenerateNewId();
	// 
	// 	// Act
	// 	var result = await repo.UpdateAsync(statusId, null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Status cannot be null.");
	// }

	// ========== CommentRepository Null Validation Tests ==========
	// NOTE: These tests are commented out because repository signatures have changed.
	// ArchiveAsync now takes ObjectId only, UpdateAsync takes CommentDto only.

	[Fact]
	public async Task CommentRepository_CreateAsync_WithNullComment_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.CreateAsync(null!, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Comment cannot be null.");
	}

	// [Fact]
	// public async Task CommentRepository_ArchiveAsync_WithNullComment_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
	// 
	// 	// Act
	// 	var result = await repo.ArchiveAsync(null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Comment cannot be null.");
	// }

	// [Fact]
	// public async Task CommentRepository_UpdateAsync_WithNullComment_ReturnsFailureResult()
	// {
	// 	// Arrange
	// 	var repo = new CommentRepository("mongodb://localhost:27017", "TestDb");
	// 	var commentId = ObjectId.GenerateNewId();
	// 
	// 	// Act
	// 	var result = await repo.UpdateAsync(commentId, null!);
	// 
	// 	// Assert
	// 	result.Success.Should().BeFalse();
	// 	result.Error.Should().Be("Comment cannot be null.");
	// }
}
