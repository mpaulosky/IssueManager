// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryApiClient.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using System.Net.Http.Json;

using Shared.DTOs;
using Shared.Validators;

namespace Web.Services;

/// <summary>Defines the contract for the Categories API client.</summary>
public interface ICategoryApiClient
{
	/// <summary>Gets all categories.</summary>
	Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>Gets a category by its identifier.</summary>
	Task<CategoryDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Creates a new category.</summary>
	Task<CategoryDto?> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);

	/// <summary>Updates an existing category.</summary>
	Task<CategoryDto?> UpdateAsync(string id, UpdateCategoryCommand command, CancellationToken cancellationToken = default);

	/// <summary>Deletes a category by its identifier.</summary>
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

/// <summary>Typed HTTP client for the Categories API.</summary>
public class CategoryApiClient : ICategoryApiClient
{
	private readonly HttpClient _httpClient;

	/// <summary>Initializes a new instance of <see cref="CategoryApiClient"/>.</summary>
	public CategoryApiClient(HttpClient httpClient) => _httpClient = httpClient;

	/// <inheritdoc/>
	public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryDto>>(
				"/api/v1/categories", cancellationToken).ConfigureAwait(false);
			return result ?? [];
		}
		catch (HttpRequestException)
		{
			return [];
		}
	}

	/// <inheritdoc/>
	public async Task<CategoryDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _httpClient.GetFromJsonAsync<CategoryDto>($"/api/v1/categories/{id}", cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public async Task<CategoryDto?> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PostAsJsonAsync("/api/v1/categories", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<CategoryDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<CategoryDto?> UpdateAsync(string id, UpdateCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.PatchAsJsonAsync($"/api/v1/categories/{id}", command, cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<CategoryDto>(cancellationToken).ConfigureAwait(false)
			: null;
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient.DeleteAsync($"/api/v1/categories/{id}", cancellationToken).ConfigureAwait(false);
		return response.IsSuccessStatusCode;
	}
}
