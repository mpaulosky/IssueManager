// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryRepositoryValidationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =============================================

namespace Api.Repositories;

/// <summary>
/// Unit tests for CategoryRepository validation logic that can be tested without a database.
/// These tests validate input validation (ObjectId.Empty checks) before any database operations occur.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CategoryRepositoryValidationTests
{
	// ========== CategoryRepository Validation Tests ==========

	[Fact]
	public async Task CategoryRepository_CreateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");
		var categoryWithEmptyId = CategoryDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.CreateAsync(categoryWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category ID cannot be empty.");
	}

	[Fact]
	public async Task CategoryRepository_ArchiveAsync_WithEmptyObjectId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.ArchiveAsync(ObjectId.Empty, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category ID cannot be empty.");
	}

	[Fact]
	public async Task CategoryRepository_UpdateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new CategoryRepository("mongodb://localhost:27017", "TestDb");
		var categoryWithEmptyId = CategoryDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.UpdateAsync(categoryWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category ID cannot be empty.");
	}
}
