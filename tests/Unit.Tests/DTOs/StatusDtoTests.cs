// Copyright (c) 2026. All rights reserved.
// File Name :     StatusDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests

using MongoDB.Bson;
using Shared.DTOs;
using Shared.Models;

namespace Tests.Unit.DTOs;

/// <summary>
/// Unit tests for <see cref="StatusDto"/>.
/// </summary>
public sealed class StatusDtoTests
{
	[Fact]
	public void Empty_ReturnsInstanceWithDefaultValues()
	{
		// Arrange / Act
		var dto = StatusDto.Empty;

		// Assert
		dto.StatusName.Should().BeEmpty();
		dto.StatusDescription.Should().BeEmpty();
		dto.Id.Should().BeNull();
		dto.DateCreated.Should().Be(default);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void Constructor_StoresAllParameters()
	{
		// Arrange
		var statusName = "Open";
		var statusDescription = "Issue is open";
		var id = ObjectId.GenerateNewId().ToString();
		var dateCreated = DateTime.UtcNow;
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto = new StatusDto(statusName, statusDescription, id, dateCreated, dateModified);

		// Assert
		dto.StatusName.Should().Be(statusName);
		dto.StatusDescription.Should().Be(statusDescription);
		dto.Id.Should().Be(id);
		dto.DateCreated.Should().Be(dateCreated);
		dto.DateModified.Should().Be(dateModified);
	}

	[Fact]
	public void Constructor_DefaultsOptionalParameters()
	{
		// Arrange
		var statusName = "Open";
		var statusDescription = "Issue is open";

		// Act
		var dto = new StatusDto(statusName, statusDescription);

		// Assert
		dto.Id.Should().BeNull();
		dto.DateCreated.Should().Be(default);
		dto.DateModified.Should().BeNull();
	}

	[Fact]
	public void ModelConstructor_MapsAllFieldsCorrectly()
	{
		// Arrange
		var status = new Status
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = "Closed",
			StatusDescription = "Issue is closed",
			DateCreated = DateTime.UtcNow,
			DateModified = DateTime.UtcNow.AddDays(3)
		};

		// Act
		var dto = new StatusDto(status);

		// Assert
		dto.StatusName.Should().Be(status.StatusName);
		dto.StatusDescription.Should().Be(status.StatusDescription);
		dto.Id.Should().Be(status.Id.ToString());
		dto.DateCreated.Should().Be(status.DateCreated);
		dto.DateModified.Should().Be(status.DateModified);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var statusName = "Open";
		var statusDescription = "Issue is open";
		var id = ObjectId.GenerateNewId().ToString();
		var dateCreated = DateTime.UtcNow;
		var dateModified = DateTime.UtcNow.AddDays(1);

		// Act
		var dto1 = new StatusDto(statusName, statusDescription, id, dateCreated, dateModified);
		var dto2 = new StatusDto(statusName, statusDescription, id, dateCreated, dateModified);

		// Assert
		dto1.Should().Be(dto2);
	}

	[Fact]
	public void RecordReferenceInequality_TwoInstancesWithSameValues_AreNotSameReference()
	{
		// Arrange
		var statusName = "Open";
		var statusDescription = "Issue is open";

		// Act
		var dto1 = new StatusDto(statusName, statusDescription);
		var dto2 = new StatusDto(statusName, statusDescription);

		// Assert
		dto1.Should().NotBeSameAs(dto2);
	}
}
