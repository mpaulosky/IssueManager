// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ResultTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests
// =======================================================

using Shared.Abstractions;

namespace Tests.Unit.Abstractions;

/// <summary>
/// Unit tests for <see cref="Result"/> and <see cref="Result{T}"/> classes.
/// </summary>
public sealed class ResultTests
{
	[Fact]
	public void Ok_ReturnsSuccessResult()
	{
		// Act
		var result = Result.Ok();

		// Assert
		result.Success.Should().BeTrue();
		result.Failure.Should().BeFalse();
		result.Error.Should().BeNull();
		result.ErrorCode.Should().Be(ResultErrorCode.None);
		result.Details.Should().BeNull();
	}

	[Fact]
	public void Fail_WithMessage_ReturnsFailureResult()
	{
		// Arrange
		const string errorMessage = "Something went wrong";

		// Act
		var result = Result.Fail(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
		result.ErrorCode.Should().Be(ResultErrorCode.None);
		result.Details.Should().BeNull();
	}

	[Fact]
	public void Fail_WithMessageAndCode_ReturnsFailureResultWithErrorCode()
	{
		// Arrange
		const string errorMessage = "Item not found";
		const ResultErrorCode errorCode = ResultErrorCode.NotFound;

		// Act
		var result = Result.Fail(errorMessage, errorCode);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be(errorMessage);
		result.ErrorCode.Should().Be(errorCode);
		result.Details.Should().BeNull();
	}

	[Fact]
	public void Fail_WithMessageCodeAndDetails_ReturnsFailureResultWithAllProperties()
	{
		// Arrange
		const string errorMessage = "Concurrency conflict";
		const ResultErrorCode errorCode = ResultErrorCode.Concurrency;
		var details = new { ServerVersion = 2, ClientVersion = 1 };

		// Act
		var result = Result.Fail(errorMessage, errorCode, details);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be(errorMessage);
		result.ErrorCode.Should().Be(errorCode);
		result.Details.Should().Be(details);
	}

	[Fact]
	public void OkGeneric_ReturnsSuccessResultWithValue()
	{
		// Arrange
		const int value = 42;

		// Act
		var result = Result.Ok(value);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
		result.Error.Should().BeNull();
		result.ErrorCode.Should().Be(ResultErrorCode.None);
	}

	[Fact]
	public void FailGeneric_WithMessage_ReturnsFailureResultWithDefaultValue()
	{
		// Arrange
		const string errorMessage = "Failed to retrieve data";

		// Act
		var result = Result.Fail<int>(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
		result.Value.Should().Be(0); // Default for int
	}

	[Fact]
	public void FailGeneric_WithMessageAndCode_ReturnsFailureResultWithErrorCode()
	{
		// Arrange
		const string errorMessage = "Validation failed";
		const ResultErrorCode errorCode = ResultErrorCode.Validation;

		// Act
		var result = Result.Fail<string>(errorMessage, errorCode);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be(errorMessage);
		result.ErrorCode.Should().Be(errorCode);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void FailGeneric_WithMessageCodeAndDetails_ReturnsFailureResultWithAllProperties()
	{
		// Arrange
		const string errorMessage = "Conflict detected";
		const ResultErrorCode errorCode = ResultErrorCode.Conflict;
		var details = new { ConflictingId = "123" };

		// Act
		var result = Result.Fail<bool>(errorMessage, errorCode, details);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be(errorMessage);
		result.ErrorCode.Should().Be(errorCode);
		result.Details.Should().Be(details);
		result.Value.Should().BeFalse(); // Default for bool
	}

	[Fact]
	public void FromValue_WithNonNullValue_ReturnsSuccessResult()
	{
		// Arrange
		const string value = "test";

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
		result.Error.Should().BeNull();
	}

	[Fact]
	public void FromValue_WithNullValue_ReturnsFailureResult()
	{
		// Arrange
		string? value = null;

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Provided value is null.");
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitOperator_ResultToValue_ReturnsValue()
	{
		// Arrange
		var result = Result.Ok(123);

		// Act
		int? value = result;

		// Assert
		value.Should().Be(123);
	}

	[Fact]
	public void ImplicitOperator_ResultToValue_WithNullResult_ReturnsDefault()
	{
		// Arrange
		Result<int>? result = null;

		// Act
		int? value = result;

		// Assert
		value.Should().Be(0); // Default for int
	}

	[Fact]
	public void ImplicitOperator_ValueToResult_ReturnsSuccessResult()
	{
		// Arrange
		const string value = "test";

		// Act
		Result<string> result = value;

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
	}

	[Fact]
	public void ImplicitOperator_NullValueToResult_ReturnsSuccessResultWithNullValue()
	{
		// Arrange
		string? value = null;

		// Act
		Result<string?> result = value;

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ResultErrorCode_HasExpectedValues()
	{
		// Assert
		((int)ResultErrorCode.None).Should().Be(0);
		((int)ResultErrorCode.Concurrency).Should().Be(1);
		((int)ResultErrorCode.NotFound).Should().Be(2);
		((int)ResultErrorCode.Validation).Should().Be(3);
		((int)ResultErrorCode.Conflict).Should().Be(4);
	}

	[Fact]
	public void Result_Failure_IsOppositeOfSuccess()
	{
		// Arrange
		var successResult = Result.Ok();
		var failureResult = Result.Fail("Error");

		// Assert
		successResult.Failure.Should().BeFalse();
		failureResult.Failure.Should().BeTrue();
	}

	[Fact]
	public void ResultGeneric_Failure_IsOppositeOfSuccess()
	{
		// Arrange
		var successResult = Result.Ok(42);
		var failureResult = Result.Fail<int>("Error");

		// Assert
		successResult.Failure.Should().BeFalse();
		failureResult.Failure.Should().BeTrue();
	}
}
