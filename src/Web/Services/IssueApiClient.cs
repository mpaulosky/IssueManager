// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueApiClient.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Shared.DTOs;
using Shared.Validators;

namespace Web.Services;

/// <summary>Defines the contract for the Issues API client.</summary>
public interface IIssueApiClient
{
	/// <summary>Gets a paginated list of issues.</summary>
	/// <param name="page">The page number (1-indexed).</param>
	/// <param name="pageSize">The number of items per page.</param>
	/// <param name="searchTerm">Optional search term to filter by title or description.</param>
	/// <param name="authorName">Optional author name to filter by.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<PaginatedResponse<IssueDto>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null, string? authorName = null, CancellationToken cancellationToken = default);

	/// <summary>Gets an issue by its identifier.</summary>
	Task<IssueDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new issue.</summary>
	Task<IssueDto?> CreateAsync(CreateIssueCommand command, CancellationToken cancellationToken = default);

	/// <summary>Updates an existing issue.</summary>
	Task<IssueDto?> UpdateAsync(string id, UpdateIssueCommand command, CancellationToken cancellationToken = default);

	/// <summary>Deletes an issue by its identifier.</summary>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>Typed HTTP client for the Issues API.</summary>
public class IssueApiClient : IIssueApiClient
{
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of <see cref="IssueApiClient"/>.</summary>
	public IssueApiClient(HttpClient httpClient) => _httpClient = httpClient;

	/// <inheritdoc/>
	public async Task<PaginatedResponse<IssueDto>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null, string? authorName = null, CancellationToken cancellationToken = default)
	{
		try
		{
			var url = $"/api/v1/issues?page={page}&pageSize={pageSize}";
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
			}
			if (!string.IsNullOrWhiteSpace(authorName))
			{
				url += $"&authorName={Uri.EscapeDataString(authorName)}";
			}

			var result = await _httpClient.GetFromJsonAsync<PaginatedResponse<IssueDto>>(url, cancellationToken).ConfigureAwait(false);
			return result ?? PaginatedResponse<IssueDto>.Empty;
		}
		catch (HttpRequestException)
		{
			return PaginatedResponse<IssueDto>.Empty;
		}
	}

	/// <inheritdoc/>
	public async Task<IssueDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<IssueDto>($"/api/v1/issues/{id}", cancellationToken).ConfigureAwait(false);
		}
		catch (HttpRequestException)
		{
			return null;
		}
	}

	/// <inheritdoc/>
	public async Task<IssueDto?> CreateAsync(CreateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PostAsJsonAsync("/api/v1/issues", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<IssueDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<IssueDto?> UpdateAsync(string id, UpdateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PatchAsJsonAsync($"/api/v1/issues/{id}", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<IssueDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.DeleteAsync($"/api/v1/issues/{id}", cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode;
	}
}
