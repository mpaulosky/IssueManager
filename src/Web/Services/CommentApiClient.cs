// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentApiClient.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Shared.DTOs;
using Shared.Validators;

namespace Web.Services;

/// <summary>Defines the contract for the Comments API client.</summary>
public interface ICommentApiClient
{
	/// <summary>Gets all comments, optionally filtered by issue ID.</summary>
	Task<IEnumerable<CommentDto>> GetAllAsync(string? issueId = null, CancellationToken cancellationToken = default);

	/// <summary>Gets a comment by its identifier.</summary>
	Task<CommentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new comment.</summary>
	Task<CommentDto?> CreateAsync(CreateCommentCommand command, CancellationToken cancellationToken = default);

	/// <summary>Updates an existing comment.</summary>
	Task<CommentDto?> UpdateAsync(string id, UpdateCommentCommand command, CancellationToken cancellationToken = default);

	/// <summary>Deletes a comment by its identifier.</summary>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>Typed HTTP client for the Comments API.</summary>
public class CommentApiClient : ICommentApiClient
{
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of <see cref="CommentApiClient"/>.</summary>
	public CommentApiClient(HttpClient httpClient) => _httpClient = httpClient;

	/// <inheritdoc/>
	public async Task<IEnumerable<CommentDto>> GetAllAsync(string? issueId = null, CancellationToken cancellationToken = default)
	{
		try
		{
			var url = "/api/v1/comments";
			if (!string.IsNullOrWhiteSpace(issueId))
			{
				url += $"?issueId={Uri.EscapeDataString(issueId)}";
			}

			var result = await _httpClient.GetFromJsonAsync<IEnumerable<CommentDto>>(
				url, cancellationToken).ConfigureAwait(false);
			return result ?? [];
		}
		catch (HttpRequestException)
		{
			return [];
		}
	}

	/// <inheritdoc/>
	public async Task<CommentDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _httpClient.GetFromJsonAsync<CommentDto>($"/api/v1/comments/{id}", cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public async Task<CommentDto?> CreateAsync(CreateCommentCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PostAsJsonAsync("/api/v1/comments", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<CommentDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<CommentDto?> UpdateAsync(string id, UpdateCommentCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PatchAsJsonAsync($"/api/v1/comments/{id}", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<CommentDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.DeleteAsync($"/api/v1/comments/{id}", cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode;
	}
}
