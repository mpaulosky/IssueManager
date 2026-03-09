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
/// These tests validate input validation and ObjectId parsing logic.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
public sealed class IssueRepositoryValidationTests
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
	// 	const string invalidId = "not-a-valid-objected";
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
}
