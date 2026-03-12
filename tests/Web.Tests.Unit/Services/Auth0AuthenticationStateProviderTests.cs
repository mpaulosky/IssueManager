// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Auth0AuthenticationStateProviderTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Unit
// =============================================

using System.Security.Claims;

namespace Web.Services;

/// <summary>
/// Unit tests for the <see cref="Auth0AuthenticationStateProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Auth0AuthenticationStateProviderTests
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<Auth0AuthenticationStateProvider> _logger;
	private readonly Auth0AuthenticationStateProvider _sut;

	public Auth0AuthenticationStateProviderTests()
	{
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_logger = Substitute.For<ILogger<Auth0AuthenticationStateProvider>>();
		_sut = new Auth0AuthenticationStateProvider(_httpContextAccessor, _logger);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithNullHttpContext_ReturnsAnonymousUser()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithUnauthenticatedUser_ReturnsAnonymousUser()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.User = new ClaimsPrincipal(new ClaimsIdentity());
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithAuthenticatedUser_NoRoles_ReturnsAuthenticatedUserWithoutRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim(ClaimTypes.Email, "test@example.com")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		result.User.IsInRole("Admin").Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithCommaSeparatedRoles_ExtractsAllRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "Admin,Author,User")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Author").Should().BeTrue();
		result.User.IsInRole("User").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithJsonArrayRoles_ExtractsAllRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "[\"Admin\",\"Author\"]")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Author").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithSingleRole_ExtractsRole()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "Admin")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithStandardRolesClaim_ExtractsRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("roles", "Admin"),
				new Claim("roles", "Author")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Author").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithBothNamespaceAndStandardRoles_CombinesRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "Admin"),
				new Claim("roles", "Author")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Author").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithDuplicateRoles_DoesNotAddDuplicates()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "Admin,Admin"),
				new Claim("roles", "Admin")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		var roleClaims = result.User.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
		roleClaims.Should().ContainSingle(c => c.Value == "Admin");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithWhitespaceInRoles_TrimsWhitespace()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "  Admin  ,  Author  ")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Author").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithEmptyRolesValue_HandlesGracefully()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithInvalidJsonArray_HandlesGracefully()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "[not valid json]")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		// Invalid JSON is logged as warning, no roles extracted
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WithEmptyJsonArray_ReturnsNoRoles()
	{
		// Arrange
		var identity = new ClaimsIdentity(
			[
				new Claim(ClaimTypes.Name, "testuser"),
				new Claim("https://articlesite.com/roles", "[]")
			],
			"TestAuth");
		var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
		_httpContextAccessor.HttpContext.Returns(context);

		// Act
		var result = await _sut.GetAuthenticationStateAsync();

		// Assert
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
	}
}
