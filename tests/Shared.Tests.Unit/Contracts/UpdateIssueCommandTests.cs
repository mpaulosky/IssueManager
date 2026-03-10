// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueCommandTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================

namespace Shared.Contracts;

/// <summary>
/// Unit tests for <see cref="UpdateIssueCommand"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class UpdateIssueCommandTests
{
	[Fact]
	public void Constructor_DefaultValues_SetsExpectedDefaults()
	{
		// Act
		var command = new UpdateIssueCommand();

		// Assert
		command.Id.Should().Be(ObjectId.Empty);
		command.Title.Should().BeEmpty();
		command.Description.Should().BeNull();
		command.ApprovedForRelease.Should().BeNull();
		command.Rejected.Should().BeNull();
	}

	[Fact]
	public void Init_WithId_SetsIdProperty()
	{
		// Arrange
		var expectedId = ObjectId.GenerateNewId();

		// Act
		var command = new UpdateIssueCommand { Id = expectedId };

		// Assert
		command.Id.Should().Be(expectedId);
	}

	[Fact]
	public void Init_WithTitle_SetsTitleProperty()
	{
		// Arrange
		const string expectedTitle = "Updated Issue Title";

		// Act
		var command = new UpdateIssueCommand { Title = expectedTitle };

		// Assert
		command.Title.Should().Be(expectedTitle);
	}

	[Fact]
	public void Init_WithDescription_SetsDescriptionProperty()
	{
		// Arrange
		const string expectedDescription = "Updated issue description with more details.";

		// Act
		var command = new UpdateIssueCommand { Description = expectedDescription };

		// Assert
		command.Description.Should().Be(expectedDescription);
	}

	[Fact]
	public void Init_WithApprovedForRelease_SetsApprovedForReleaseProperty()
	{
		// Arrange / Act
		var commandApproved = new UpdateIssueCommand { ApprovedForRelease = true };
		var commandNotApproved = new UpdateIssueCommand { ApprovedForRelease = false };

		// Assert
		commandApproved.ApprovedForRelease.Should().BeTrue();
		commandNotApproved.ApprovedForRelease.Should().BeFalse();
	}

	[Fact]
	public void Init_WithRejected_SetsRejectedProperty()
	{
		// Arrange / Act
		var commandRejected = new UpdateIssueCommand { Rejected = true };
		var commandNotRejected = new UpdateIssueCommand { Rejected = false };

		// Assert
		commandRejected.Rejected.Should().BeTrue();
		commandNotRejected.Rejected.Should().BeFalse();
	}

	[Fact]
	public void Init_WithAllProperties_SetsAllPropertiesCorrectly()
	{
		// Arrange
		var expectedId = ObjectId.GenerateNewId();
		const string expectedTitle = "Complete Issue Update";
		const string expectedDescription = "Full description for the updated issue.";
		const bool expectedApproved = true;
		const bool expectedRejected = false;

		// Act
		var command = new UpdateIssueCommand
		{
			Id = expectedId,
			Title = expectedTitle,
			Description = expectedDescription,
			ApprovedForRelease = expectedApproved,
			Rejected = expectedRejected
		};

		// Assert
		command.Id.Should().Be(expectedId);
		command.Title.Should().Be(expectedTitle);
		command.Description.Should().Be(expectedDescription);
		command.ApprovedForRelease.Should().Be(expectedApproved);
		command.Rejected.Should().Be(expectedRejected);
	}

	[Fact]
	public void RecordValueEquality_TwoInstancesWithSameValues_AreEqual()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		const string title = "Same Title";
		const string description = "Same Description";

		// Act
		var command1 = new UpdateIssueCommand
		{
			Id = id,
			Title = title,
			Description = description,
			ApprovedForRelease = true,
			Rejected = false
		};
		var command2 = new UpdateIssueCommand
		{
			Id = id,
			Title = title,
			Description = description,
			ApprovedForRelease = true,
			Rejected = false
		};

		// Assert
		command1.Should().Be(command2);
	}

	[Fact]
	public void RecordValueInequality_TwoInstancesWithDifferentValues_AreNotEqual()
	{
		// Arrange
		var command1 = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Title One"
		};
		var command2 = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Title Two"
		};

		// Assert
		command1.Should().NotBe(command2);
	}
}
