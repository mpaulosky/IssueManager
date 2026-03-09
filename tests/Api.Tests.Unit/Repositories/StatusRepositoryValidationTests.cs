// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusRepositoryValidationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =============================================

namespace Api.Repositories;

/// <summary>
/// Unit tests for StatusRepository validation logic that can be tested without a database.
/// These tests validate input validation and ObjectId parsing logic.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
public sealed class StatusRepositoryValidationTests
{
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
}
