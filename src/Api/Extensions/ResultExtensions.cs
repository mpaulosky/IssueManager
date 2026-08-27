// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ResultExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Extensions;

/// <summary>
/// The single seam that maps a <see cref="Result{T}"/> outcome to an HTTP response, and a
/// route-id parsing helper. Every endpoint in every *Endpoints.cs file should use these
/// instead of hand-rolling its own Result-to-status branching or ObjectId parsing.
/// </summary>
public static class ResultExtensions
{
	/// <summary>
	/// Maps a <see cref="Result{T}"/> to an <see cref="IResult"/>. On success, delegates to
	/// <paramref name="onSuccess"/> so each route can pick its own success status (200, 201, 204, ...).
	/// On failure, maps <see cref="Result{T}.ErrorCode"/> to the canonical HTTP status: NotFound to 404,
	/// Validation to 400, Conflict and Concurrency to 409. A failed Result carrying
	/// <see cref="ResultErrorCode.None"/> is a contract violation - every handler is expected to set a
	/// real error code on failure - so that case throws rather than silently returning a status code.
	/// </summary>
	public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
	{
		if (result.Success)
			return onSuccess(result.Value!);

		return result.ErrorCode switch
		{
			ResultErrorCode.NotFound => Results.NotFound(),
			ResultErrorCode.Validation => Results.BadRequest(result.Error),
			ResultErrorCode.Conflict => Results.Conflict(result.Error),
			ResultErrorCode.Concurrency => Results.Conflict(result.Error),
			_ => throw new InvalidOperationException(
				$"Result failed without a recognized error code (was '{result.ErrorCode}'). Every failing Result must set a real ResultErrorCode.")
		};
	}

	/// <summary>
	/// Parses a route-supplied id string to an <see cref="ObjectId"/>. Returns <see langword="false"/>
	/// and sets <paramref name="badRequest"/> to a 400 response when the id is not a valid ObjectId.
	/// </summary>
	public static bool TryParseObjectIdOrBadRequest(this string id, out ObjectId objectId, out IResult badRequest)
	{
		if (ObjectId.TryParse(id, out objectId))
		{
			badRequest = Results.Empty;
			return true;
		}

		badRequest = Results.BadRequest("Invalid ID format");
		return false;
	}
}
