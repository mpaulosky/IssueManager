// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Api.Extensions;

/// <summary>
/// Unit tests for AuthExtensions, NoAuthHandler, and NoAuthOptions.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthExtensionsTests
{
	[Fact]
	public void NoAuthOptions_IsAuthenticationSchemeOptions()
	{
		// Arrange & Act
		var options = new NoAuthOptions();

		// Assert
		options.Should().BeAssignableTo<AuthenticationSchemeOptions>();
	}

	[Fact]
	public async Task NoAuthHandler_HandleAuthenticateAsync_ReturnsSuccessWithNoAuthIdentity()
	{
		// Arrange
		var options = new NoAuthOptions();
		var optionsMonitor = Substitute.For<IOptionsMonitor<NoAuthOptions>>();
		optionsMonitor.Get(Arg.Any<string>()).Returns(options);

		var loggerFactory = Substitute.For<ILoggerFactory>();
		loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

		var encoder = UrlEncoder.Default;

		var handler = new NoAuthHandler(optionsMonitor, loggerFactory, encoder);

		// Create a minimal HttpContext
		var httpContext = new DefaultHttpContext();
		var scheme = new AuthenticationScheme("NoAuth", "NoAuth", typeof(NoAuthHandler));

		await handler.InitializeAsync(scheme, httpContext);

		// Act
		var result = await handler.AuthenticateAsync();

		// Assert
		result.Succeeded.Should().BeTrue();
		result.Principal.Should().NotBeNull();
		result.Principal!.Identity.Should().NotBeNull();
		result.Principal!.Identity!.AuthenticationType.Should().Be("NoAuth");
		result.Ticket.Should().NotBeNull();
		result.Ticket!.AuthenticationScheme.Should().Be("NoAuth");
	}

	[Fact]
	public async Task NoAuthHandler_HandleAuthenticateAsync_ReturnsClaimsPrincipal()
	{
		// Arrange
		var options = new NoAuthOptions();
		var optionsMonitor = Substitute.For<IOptionsMonitor<NoAuthOptions>>();
		optionsMonitor.Get(Arg.Any<string>()).Returns(options);

		var loggerFactory = Substitute.For<ILoggerFactory>();
		loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

		var encoder = UrlEncoder.Default;

		var handler = new NoAuthHandler(optionsMonitor, loggerFactory, encoder);

		var httpContext = new DefaultHttpContext();
		var scheme = new AuthenticationScheme("NoAuth", "NoAuth", typeof(NoAuthHandler));

		await handler.InitializeAsync(scheme, httpContext);

		// Act
		var result = await handler.AuthenticateAsync();

		// Assert
		result.Principal.Should().BeOfType<ClaimsPrincipal>();
		result.Principal!.Identities.Should().ContainSingle();
	}

	[Fact]
	public void NoAuthOptions_CanSetValidateOptions()
	{
		// Arrange & Act
		var options = new NoAuthOptions
		{
			ClaimsIssuer = "TestIssuer"
		};

		// Assert
		options.ClaimsIssuer.Should().Be("TestIssuer");
	}

	[Fact]
	public void NoAuthHandler_CanBeConstructed()
	{
		// Arrange
		var optionsMonitor = Substitute.For<IOptionsMonitor<NoAuthOptions>>();
		optionsMonitor.Get(Arg.Any<string>()).Returns(new NoAuthOptions());

		var loggerFactory = Substitute.For<ILoggerFactory>();
		loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

		var encoder = UrlEncoder.Default;

		// Act
		var handler = new NoAuthHandler(optionsMonitor, loggerFactory, encoder);

		// Assert
		handler.Should().NotBeNull();
	}
}
