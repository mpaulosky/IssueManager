// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data.Interfaces;

/// <summary>
/// The CRUD contract every entity repository shares: archive (soft-delete), create, get by id,
/// get all, update, and count. Entity-specific repositories extend this with their own
/// distinctive query methods (filtered/paginated listing, lookups, etc.).
/// </summary>
public interface IRepository<TDto>
{
	/// <summary>
	/// Soft-deletes an entity by setting Archived to true.
	/// </summary>
	Task<Result> ArchiveAsync(ObjectId id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new entity in the database.
	/// </summary>
	Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an entity by its unique identifier.
	/// </summary>
	Task<Result<TDto>> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all entities from the database.
	/// </summary>
	Task<Result<IReadOnlyList<TDto>>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing entity in the database.
	/// </summary>
	Task<Result<TDto>> UpdateAsync(TDto dto, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of entities in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);
}
