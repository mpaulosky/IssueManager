// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ResultExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Extensions;

/// <summary>
/// Unit tests for ResultExtensions - the single seam that maps a Result{T} to an HTTP response.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResultExtensionsTests
{
	[Fact]
	public void ToHttpResult_Success_InvokesOnSuccessWithValue()
	{
		// Arrange
		var result = Result.Ok("value");

		// Act
		var httpResult = result.ToHttpResult(value => Results.Ok(value));

		// Assert
		httpResult.Should().BeOfType<Ok<string>>();
		((Ok<string>)httpResult).Value.Should().Be("value");
	}

	[Fact]
	public void ToHttpResult_NotFound_Returns404()
	{
		// Arrange
		var result = Result.Fail<string>("Not found.", ResultErrorCode.NotFound);

		// Act
		var httpResult = result.ToHttpResult(Results.Ok);

		// Assert
		httpResult.Should().BeOfType<NotFound>();
	}

	[Fact]
	public void ToHttpResult_Validation_Returns400WithErrorMessage()
	{
		// Arrange
		var result = Result.Fail<string>("Name is required.", ResultErrorCode.Validation);

		// Act
		var httpResult = result.ToHttpResult(Results.Ok);

		// Assert
		httpResult.Should().BeOfType<BadRequest<string>>();
		((BadRequest<string>)httpResult).Value.Should().Be("Name is required.");
	}

	[Fact]
	public void ToHttpResult_Conflict_Returns409WithErrorMessage()
	{
		// Arrange
		var result = Result.Fail<string>("Already archived.", ResultErrorCode.Conflict);

		// Act
		var httpResult = result.ToHttpResult(Results.Ok);

		// Assert
		httpResult.Should().BeOfType<Conflict<string>>();
		((Conflict<string>)httpResult).Value.Should().Be("Already archived.");
	}

	[Fact]
	public void ToHttpResult_Concurrency_Returns409WithErrorMessage()
	{
		// Arrange
		var result = Result.Fail<string>("Version mismatch.", ResultErrorCode.Concurrency);

		// Act
		var httpResult = result.ToHttpResult(Results.Ok);

		// Assert
		httpResult.Should().BeOfType<Conflict<string>>();
		((Conflict<string>)httpResult).Value.Should().Be("Version mismatch.");
	}

	[Fact]
	public void ToHttpResult_NoErrorCode_ThrowsInvalidOperationException()
	{
		// Arrange - a failed Result carrying ResultErrorCode.None is a contract violation
		var result = Result.Fail<string>("Something went wrong.");

		// Act
		var act = () => result.ToHttpResult(Results.Ok);

		// Assert
		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void TryParseObjectIdOrBadRequest_ValidId_ReturnsTrue()
	{
		// Arrange
		var id = ObjectId.GenerateNewId().ToString();

		// Act
		var parsed = id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest);

		// Assert
		parsed.Should().BeTrue();
		objectId.ToString().Should().Be(id);
		badRequest.Should().NotBeNull();
	}

	[Fact]
	public void TryParseObjectIdOrBadRequest_InvalidId_ReturnsFalseWithBadRequest()
	{
		// Arrange
		const string id = "not-a-valid-objectid";

		// Act
		var parsed = id.TryParseObjectIdOrBadRequest(out _, out var badRequest);

		// Assert
		parsed.Should().BeFalse();
		badRequest.Should().BeOfType<BadRequest<string>>();
		((BadRequest<string>)badRequest).Value.Should().Be("Invalid ID format");
	}
}
