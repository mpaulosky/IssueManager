// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CurrentUserServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using System.Security.Claims;
using Api.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Tests.Unit.Services;

/// <summary>
/// Unit tests for CurrentUserService.
/// </summary>
public class CurrentUserServiceTests
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserServiceTests()
	{
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
	}

	[Fact]
	public void UserId_WhenUserHasSubClaim_ReturnsSubValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "nameidentifier-123"),
			new("sub", "auth0|123456")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.UserId;

		// Assert
		result.Should().Be("nameidentifier-123");
	}

	[Fact]
	public void UserId_WhenUserHasOnlySubClaim_ReturnsSubValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new("sub", "auth0|123456")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.UserId;

		// Assert
		result.Should().Be("auth0|123456");
	}

	[Fact]
	public void UserId_WhenUserHasNameIdentifierClaim_ReturnsNameIdentifierValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "nameidentifier-123")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.UserId;

		// Assert
		result.Should().Be("nameidentifier-123");
	}

	[Fact]
	public void Name_WhenUserHasNameClaim_ReturnsNameValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.Name, "John Doe"),
			new("name", "Jane Doe")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Name;

		// Assert
		result.Should().Be("John Doe");
	}

	[Fact]
	public void Name_WhenUserHasLowercaseNameClaim_ReturnsLowercaseNameValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new("name", "Jane Doe")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Name;

		// Assert
		result.Should().Be("Jane Doe");
	}

	[Fact]
	public void Email_WhenUserHasEmailClaim_ReturnsEmailValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.Email, "john@example.com"),
			new("email", "jane@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Email;

		// Assert
		result.Should().Be("john@example.com");
	}

	[Fact]
	public void Email_WhenUserHasLowercaseEmailClaim_ReturnsLowercaseEmailValue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new("email", "jane@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Email;

		// Assert
		result.Should().Be("jane@example.com");
	}

	[Fact]
	public void IsAuthenticated_WhenUserIsAuthenticated_ReturnsTrue()
	{
		// Arrange
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, "user123")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.IsAuthenticated;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsAuthenticated_WhenUserIsNotAuthenticated_ReturnsFalse()
	{
		// Arrange
		var identity = new ClaimsIdentity(); // No authentication type
		var principal = new ClaimsPrincipal(identity);
		var httpContext = new DefaultHttpContext { User = principal };
		_httpContextAccessor.HttpContext.Returns(httpContext);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.IsAuthenticated;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void UserId_WhenHttpContextIsNull_ReturnsNull()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.UserId;

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void Name_WhenHttpContextIsNull_ReturnsNull()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Name;

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void Email_WhenHttpContextIsNull_ReturnsNull()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.Email;

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void IsAuthenticated_WhenHttpContextIsNull_ReturnsFalse()
	{
		// Arrange
		_httpContextAccessor.HttpContext.Returns((HttpContext?)null);

		var service = new CurrentUserService(_httpContextAccessor);

		// Act
		var result = service.IsAuthenticated;

		// Assert
		result.Should().BeFalse();
	}
}
