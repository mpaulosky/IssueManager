// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     NotFoundExceptionTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared.Tests.Unit
// =============================================

namespace Shared.Exceptions;

/// <summary>
/// Unit tests for <see cref="NotFoundException"/>.
/// </summary>
public sealed class NotFoundExceptionTests
{
	[Fact]
	public void MessageConstructor_SetsMessage()
	{
		// Arrange
		var message = "Resource not found";

		// Act
		var exception = new NotFoundException(message);

		// Assert
		exception.Message.Should().Be(message);
	}

	[Fact]
	public void MessageAndInnerExceptionConstructor_SetsBothProperties()
	{
		// Arrange
		var message = "Resource not found";
		var innerException = new InvalidOperationException("Inner error");

		// Act
		var exception = new NotFoundException(message, innerException);

		// Assert
		exception.Message.Should().Be(message);
		exception.InnerException.Should().Be(innerException);
	}

	[Fact]
	public void Exception_IsAssignableToBaseException()
	{
		// Arrange
		var exception = new NotFoundException("Test message");

		// Act / Assert
		exception.Should().BeAssignableTo<Exception>();
	}

	[Fact]
	public void MessageConstructor_MessageMatchesExactly()
	{
		// Arrange
		var expectedMessage = "Item with ID 123 not found";

		// Act
		var exception = new NotFoundException(expectedMessage);

		// Assert
		exception.Message.Should().Be(expectedMessage);
	}
}
