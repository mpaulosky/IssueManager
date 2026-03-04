// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Authorization;

using Web.Extensions;

namespace BlazorTests.Extensions;

/// <summary>
/// Unit tests for <see cref="AuthExtensions"/> extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthExtensionsTests
{
	[Fact]
	public void AddAuth0_WithoutConfig_ReturnsBuilderWithoutAuthRegistered()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		// No Auth0: Domain or Auth0: ClientId configured

		// Act
		var result = builder.AddAuth0();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeSameAs(builder);
		// Build to check services — Auth0 is not configured, so only minimal services exist
		using var app = builder.Build();
		app.Services.GetService<IAuthorizationService>().Should().BeNull();
	}

	[Fact]
	public void AddAuth0_WithConfig_RegistersAuthenticationServices()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		builder.Configuration["Auth0:Domain"] = "test.auth0.com";
		builder.Configuration["Auth0:ClientId"] = "test-client-id";
		builder.Configuration["Auth0:ClientSecret"] = "test-secret";

		// Act
		var result = builder.AddAuth0();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeSameAs(builder);
		// Build and check that the authorization service is registered
		using var app = builder.Build();
		app.Services.GetService<IAuthorizationService>().Should().NotBeNull();
	}
}
