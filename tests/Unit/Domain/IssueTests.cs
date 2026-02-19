using FluentAssertions;
using IssueManager.Shared.Domain;

namespace IssueManager.Tests.Unit.Domain;

/// <summary>
/// Unit tests for <see cref="Issue"/>.
/// </summary>
public class IssueTests
{
	[Fact]
	public void Issue_Create_GeneratesValidIssueWithOpenStatus()
	{
		// Arrange
		var title = "Test Issue";
		var description = "Test Description";

		// Act
		var issue = Issue.Create(title, description);

		// Assert
		issue.Should().NotBeNull();
		issue.Id.Should().NotBeNullOrEmpty();
		issue.Title.Should().Be(title);
		issue.Description.Should().Be(description);
		issue.Status.Should().Be(IssueStatus.Open);
		issue.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		issue.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		issue.Labels.Should().BeEmpty();
	}

	[Fact]
	public void Issue_Create_WithLabels_AssignsLabelsCorrectly()
	{
		// Arrange
		var title = "Test Issue";
		var labels = new List<Label>
		{
			new("bug", "#FF0000"),
			new("critical", "#FFA500")
		};

		// Act
		var issue = Issue.Create(title, null, labels);

		// Assert
		issue.Labels.Should().HaveCount(2);
		issue.Labels.Should().Contain(l => l.Name == "bug");
		issue.Labels.Should().Contain(l => l.Name == "critical");
	}

	[Fact]
	public void Issue_UpdateStatus_ChangesStatusAndUpdatesTimestamp()
	{
		// Arrange
		var issue = Issue.Create("Test", "Description");
		var originalUpdatedAt = issue.UpdatedAt;
		Thread.Sleep(10);

		// Act
		var updatedIssue = issue.UpdateStatus(IssueStatus.InProgress);

		// Assert
		updatedIssue.Status.Should().Be(IssueStatus.InProgress);
		updatedIssue.UpdatedAt.Should().BeAfter(originalUpdatedAt);
		updatedIssue.Id.Should().Be(issue.Id);
		updatedIssue.Title.Should().Be(issue.Title);
	}

	[Fact]
	public void Issue_UpdateStatus_WithSameStatus_ReturnsSameInstance()
	{
		// Arrange
		var issue = Issue.Create("Test", "Description");

		// Act
		var updatedIssue = issue.UpdateStatus(IssueStatus.Open);

		// Assert
		updatedIssue.Should().BeSameAs(issue);
	}

	[Fact]
	public void Issue_Update_ChangesTitleDescriptionAndUpdatesTimestamp()
	{
		// Arrange
		var issue = Issue.Create("Original Title", "Original Description");
		var originalUpdatedAt = issue.UpdatedAt;
		Thread.Sleep(10);

		// Act
		var updatedIssue = issue.Update("New Title", "New Description");

		// Assert
		updatedIssue.Title.Should().Be("New Title");
		updatedIssue.Description.Should().Be("New Description");
		updatedIssue.UpdatedAt.Should().BeAfter(originalUpdatedAt);
		updatedIssue.Id.Should().Be(issue.Id);
		updatedIssue.Status.Should().Be(issue.Status);
	}

	[Fact]
	public void Issue_Constructor_WithEmptyId_ThrowsArgumentException()
	{
		// Act
		var act = () => new Issue("", "Title", "Desc", IssueStatus.Open, DateTime.UtcNow, DateTime.UtcNow);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*Issue ID cannot be empty*");
	}

	[Fact]
	public void Issue_Constructor_WithEmptyTitle_ThrowsArgumentException()
	{
		// Act
		var act = () => new Issue("id-123", "", "Desc", IssueStatus.Open, DateTime.UtcNow, DateTime.UtcNow);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*Issue title cannot be empty*");
	}

	[Fact]
	public void Issue_RecordEquality_WithSameValues_AreEqual()
	{
		// Arrange
		var dateTime = DateTime.UtcNow;
		var issue1 = new Issue("id-1", "Title", "Desc", IssueStatus.Open, dateTime, dateTime);
		var issue2 = new Issue("id-1", "Title", "Desc", IssueStatus.Open, dateTime, dateTime);

		// Act & Assert
		issue1.Should().Be(issue2);
		(issue1 == issue2).Should().BeTrue();
	}
}
