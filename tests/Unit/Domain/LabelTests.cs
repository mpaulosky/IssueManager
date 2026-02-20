using FluentAssertions;

using Shared.Domain;

namespace IssueManager.Tests.Unit.Domain;

/// <summary>
/// Unit tests for <see cref="Label"/>.
/// </summary>
public class LabelTests
{
	[Fact]
	public void Label_Constructor_WithValidValues_CreatesLabel()
	{
		// Arrange
		var name = "bug";
		var color = "#FF0000";

		// Act
		var label = new Label(name, color);

		// Assert
		label.Name.Should().Be(name);
		label.Color.Should().Be(color);
	}

	[Fact]
	public void Label_Constructor_WithEmptyName_ThrowsArgumentException()
	{
		// Act
		var act = () => new Label("", "#FF0000");

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*Label name cannot be empty*");
	}

	[Fact]
	public void Label_Constructor_WithEmptyColor_ThrowsArgumentException()
	{
		// Act
		var act = () => new Label("bug", "");

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*Label color cannot be empty*");
	}

	[Fact]
	public void Label_RecordEquality_WithSameValues_AreEqual()
	{
		// Arrange
		var label1 = new Label("bug", "#FF0000");
		var label2 = new Label("bug", "#FF0000");

		// Act & Assert
		label1.Should().Be(label2);
		(label1 == label2).Should().BeTrue();
	}

	[Fact]
	public void Label_RecordEquality_WithDifferentValues_AreNotEqual()
	{
		// Arrange
		var label1 = new Label("bug", "#FF0000");
		var label2 = new Label("feature", "#00FF00");

		// Act & Assert
		label1.Should().NotBe(label2);
		(label1 != label2).Should().BeTrue();
	}
}
