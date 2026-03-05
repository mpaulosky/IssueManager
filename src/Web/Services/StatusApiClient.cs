// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusApiClient.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Shared.DTOs;
using Shared.Validators;

namespace Web.Services;

/// <summary>Defines the contract for the Statuses API client.</summary>
public interface IStatusApiClient
{
	/// <summary>Gets all statuses.</summary>
	Task<IEnumerable<StatusDto>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets a status by its identifier.</summary>
	Task<StatusDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new status.</summary>
	Task<StatusDto?> CreateAsync(CreateStatusCommand command, CancellationToken cancellationToken = default);

	/// <summary>Updates an existing status.</summary>
	Task<StatusDto?> UpdateAsync(string id, UpdateStatusCommand command, CancellationToken cancellationToken = default);
}

/// <summary>Typed HTTP client for the Statuses API.</summary>
public class StatusApiClient : IStatusApiClient
{
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of <see cref="StatusApiClient"/>.</summary>
	public StatusApiClient(HttpClient httpClient) => _httpClient = httpClient;

	/// <inheritdoc/>
	public async Task<IEnumerable<StatusDto>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<IEnumerable<StatusDto>>(
				"/api/v1/statuses", cancellationToken).ConfigureAwait(false);
			return result ?? [];
		}
		catch (HttpRequestException)
		{
			return [];
		}
	}

	/// <inheritdoc/>
	public async Task<StatusDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _httpClient.GetFromJsonAsync<StatusDto>($"/api/v1/statuses/{id}", cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public async Task<StatusDto?> CreateAsync(CreateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PostAsJsonAsync("/api/v1/statuses", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<StatusDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<StatusDto?> UpdateAsync(string id, UpdateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PatchAsJsonAsync($"/api/v1/statuses/{id}", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<StatusDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

}
