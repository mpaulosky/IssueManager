// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ConflictExceptionTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================

namespace Shared.Exceptions;

/// <summary>
/// Unit tests for <see cref="ConflictException"/>.
/// </summary>
public sealed class ConflictExceptionTests
{
	[Fact]
	public void MessageConstructor_SetsMessage()
	{
		// Arrange
		var message = "Resource conflict detected";

		// Act
		var exception = new ConflictException(message);

		// Assert
		exception.Message.Should().Be(message);
	}

	[Fact]
	public void MessageAndInnerExceptionConstructor_SetsBothProperties()
	{
		// Arrange
		var message = "Resource conflict detected";
		var innerException = new InvalidOperationException("Inner error");

		// Act
		var exception = new ConflictException(message, innerException);

		// Assert
		exception.Message.Should().Be(message);
		exception.InnerException.Should().Be(innerException);
	}

	[Fact]
	public void Exception_IsAssignableToBaseException()
	{
		// Arrange
		var exception = new ConflictException("Test message");

		// Act / Assert
		exception.Should().BeAssignableTo<Exception>();
	}

	[Fact]
	public void MessageConstructor_MessageMatchesExactly()
	{
		// Arrange
		var expectedMessage = "Item with ID 456 already exists";

		// Act
		var exception = new ConflictException(expectedMessage);

		// Assert
		exception.Message.Should().Be(expectedMessage);
	}
}
