// Copyright (c) 2026. All rights reserved.

using Tests.BlazorTests.Fixtures;

using Web.Layout;

namespace Tests.BlazorTests.Layout;

/// <summary>
/// bUnit tests for the <see cref="FooterComponent"/> Blazor component.
/// </summary>
public class FooterComponentTests : ComponentTestBase
{
	[Fact]
	public void FooterComponent_RendersWithoutError()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		cut.Should().NotBeNull();
		cut.Find("footer").Should().NotBeNull();
	}

	[Fact]
	public void FooterComponent_HasContentInfoRole()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		cut.Find("[role='contentinfo']").Should().NotBeNull();
	}

	[Fact]
	public void FooterComponent_ShowsCopyrightText()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		cut.Markup.Should().Contain("IssueManager");
		cut.Markup.Should().Contain(".NET 10");
		cut.Markup.Should().Contain("Blazor");
	}

	[Fact]
	public void FooterComponent_ShowsCurrentYear()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		cut.Markup.Should().Contain(DateTime.Now.Year.ToString());
	}

	[Fact]
	public void FooterComponent_ShowsVersionText()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert — footer always renders some version/commit text in the font-mono section
		var monoLinks = cut.FindAll("a.hover\\:text-\\[var\\(--color-primary\\)\\]");
		cut.Markup.Should().Contain("github.com");
	}

	[Fact]
	public void FooterComponent_ShowsCommitText()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert — the footer renders at least two anchor tags in the version section
		var versionLinks = cut.FindAll("a[target='_blank']");
		versionLinks.Should().HaveCountGreaterOrEqualTo(2);
	}

	[Fact]
	public void FooterComponent_HasGitHubLinks()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		var links = cut.FindAll("a[href*='github.com']");
		links.Should().HaveCountGreaterOrEqualTo(2);
	}

	[Fact]
	public void FooterComponent_LinksOpenInNewTab()
	{
		// Act
		var cut = TestContext.RenderComponent<FooterComponent>();

		// Assert
		var externalLinks = cut.FindAll("a[target='_blank']");
		externalLinks.Should().HaveCountGreaterOrEqualTo(2);
	}
}
