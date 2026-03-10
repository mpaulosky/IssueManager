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
/// These tests validate input validation (ObjectId.Empty checks) before any database operations occur.
/// Note: Integration tests cover full CRUD operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class StatusRepositoryValidationTests
{
	// ========== StatusRepository Validation Tests ==========

	[Fact]
	public async Task StatusRepository_CreateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");
		var statusWithEmptyId = StatusDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.CreateAsync(statusWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Status ID cannot be empty.");
	}

	[Fact]
	public async Task StatusRepository_ArchiveAsync_WithEmptyObjectId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");

		// Act
		var result = await repo.ArchiveAsync(ObjectId.Empty, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Status ID cannot be empty.");
	}

	[Fact]
	public async Task StatusRepository_UpdateAsync_WithEmptyId_ReturnsFailureResult()
	{
		// Arrange
		var repo = new StatusRepository("mongodb://localhost:27017", "TestDb");
		var statusWithEmptyId = StatusDto.Empty with { Id = ObjectId.Empty };

		// Act
		var result = await repo.UpdateAsync(statusWithEmptyId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Status ID cannot be empty.");
	}
}
