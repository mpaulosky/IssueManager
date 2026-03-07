// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusEditModelTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Unit
// =============================================

namespace Web.Components.Features.Statuses;

/// <summary>
/// Unit tests for <see cref="StatusEditModel"/> default values and property setters.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusEditModelTests
{
	[Fact]
	public void Constructor_SetsExpectedDefaultValues()
	{
		// Arrange / Act
		var model = new StatusEditModel();

		// Assert
		model.Id.Should().Be("");
		model.StatusName.Should().Be("");
		model.StatusDescription.Should().BeNull();
	}

	[Fact]
	public void Id_CanBeSet()
	{
		// Arrange
		var model = new StatusEditModel();

		// Act
		model.Id = "abc123";

		// Assert
		model.Id.Should().Be("abc123");
	}

	[Fact]
	public void StatusName_CanBeSet()
	{
		// Arrange
		var model = new StatusEditModel();

		// Act
		model.StatusName = "Open";

		// Assert
		model.StatusName.Should().Be("Open");
	}

	[Fact]
	public void StatusDescription_CanBeSet()
	{
		// Arrange
		var model = new StatusEditModel();

		// Act
		model.StatusDescription = "Issue is open and awaiting triage";

		// Assert
		model.StatusDescription.Should().Be("Issue is open and awaiting triage");
	}

	[Fact]
	public void StatusDescription_CanBeSetToNull()
	{
		// Arrange
		var model = new StatusEditModel { StatusDescription = "Some description" };

		// Act
		model.StatusDescription = null;

		// Assert
		model.StatusDescription.Should().BeNull();
	}
}
