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
/// These tests validate input validation and ObjectId parsing logic.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
public sealed class CategoryRepositoryValidationTests
{
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
}
