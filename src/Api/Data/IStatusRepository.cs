// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     IStatusRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Api.Data;

/// <summary>
/// Provides data access operations for status entities in the data store.
/// </summary>
public interface IStatusRepository
{
	/// <summary>
	/// Archives a status by marking it as inactive in the data store.
	/// </summary>
	/// <param name="status">The status to archive.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is null.</exception>
	Task<Result> ArchiveAsync(Status status);

	/// <summary>
	/// Creates a new status in the data store.
	/// </summary>
	/// <param name="status">The status to create.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is null.</exception>
	Task<Result> CreateAsync(Status status);

	/// <summary>
	/// Retrieves a specific status from the data store by its unique identifier.
	/// </summary>
	/// <param name="itemId">The unique identifier of the status.</param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains the requested <see cref="Status"/>.
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="itemId"/> is null or empty.</exception>
	Task<Result<Status>> GetAsync(ObjectId itemId);

	/// <summary>
	/// Retrieves all statuses from the data store.
	/// </summary>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains an enumerable collection of all <see cref="Status"/> instances.
	/// </returns>
	Task<Result<IEnumerable<Status>>> GetAllAsync();

	/// <summary>
	/// Updates an existing status in the data store.
	/// </summary>
	/// <param name="itemId">The unique identifier of the status to update.</param>
	/// <param name="status">The updated status data.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="itemId"/> is null or empty.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is null.</exception>
	Task<Result> UpdateAsync(ObjectId itemId, Status status);
}
