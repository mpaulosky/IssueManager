// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusMapperTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using MongoDB.Bson;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;

namespace Tests.Unit.Mappers;

/// <summary>
/// Unit tests for StatusMapper extension methods.
/// </summary>
public class StatusMapperTests
{
	[Fact]
	public void ToDto_ShouldMapAllFields_FromStatusModel()
	{
		// Arrange
		var status = new Status
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = "Open",
			StatusDescription = "Issue is open",
			DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			DateModified = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = status.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.StatusName.Should().Be(status.StatusName);
		dto.StatusDescription.Should().Be(status.StatusDescription);
		dto.Id.Should().Be(status.Id.ToString());
		dto.DateCreated.Should().Be(status.DateCreated);
		dto.DateModified.Should().Be(status.DateModified);
	}

	[Fact]
	public void ToDto_ShouldConvertObjectIdToString()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var status = new Status
		{
			Id = statusId,
			StatusName = "Closed",
			StatusDescription = "Issue is closed",
			DateCreated = DateTime.UtcNow
		};

		// Act
		var dto = status.ToDto();

		// Assert
		dto.Id.Should().Be(statusId.ToString());
	}

	[Fact]
	public void ToDto_ShouldHandleNullDateModified()
	{
		// Arrange
		var status = new Status
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = "In Progress",
			StatusDescription = "Work in progress",
			DateCreated = DateTime.UtcNow,
			DateModified = null
		};

		// Act
		var dto = status.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToModel_ShouldMapAllFields_FromStatusDto()
	{
		// Arrange
		var dto = new StatusDto(
			"Done",
			"Task is complete",
			ObjectId.GenerateNewId().ToString(),
			new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc));

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.StatusName.Should().Be(dto.StatusName);
		model.StatusDescription.Should().Be(dto.StatusDescription);
		model.DateCreated.Should().Be(dto.DateCreated);
		model.DateModified.Should().Be(dto.DateModified);
	}

	[Fact]
	public void ToModel_ShouldHandleNullDateModified()
	{
		// Arrange
		var dto = new StatusDto(
			"Pending",
			"Waiting for approval",
			ObjectId.GenerateNewId().ToString(),
			DateTime.UtcNow,
			null);

		// Act
		var model = dto.ToModel();

		// Assert
		model.Should().NotBeNull();
		model.DateModified.Should().BeNull();
	}

	[Fact]
	public void ToDto_ToModel_ShouldRoundTrip()
	{
		// Arrange
		var original = new Status
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = "Blocked",
			StatusDescription = "Issue is blocked",
			DateCreated = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
			DateModified = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
		};

		// Act
		var dto = original.ToDto();
		var roundTripped = dto.ToModel();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.StatusName.Should().Be(original.StatusName);
		roundTripped.StatusDescription.Should().Be(original.StatusDescription);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}

	[Fact]
	public void ToModel_ToDto_ShouldRoundTrip()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var original = new StatusDto(
			"Review",
			"Under review",
			statusId.ToString(),
			new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
			new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc));

		// Act
		var model = original.ToModel();
		var roundTripped = model.ToDto();

		// Assert
		roundTripped.Should().NotBeNull();
		roundTripped.StatusName.Should().Be(original.StatusName);
		roundTripped.StatusDescription.Should().Be(original.StatusDescription);
		roundTripped.DateCreated.Should().Be(original.DateCreated);
		roundTripped.DateModified.Should().Be(original.DateModified);
	}
}
