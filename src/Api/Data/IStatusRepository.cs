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
	/// Soft-deletes a status by setting Archived to true.
	/// </summary>
	Task<Result> ArchiveAsync(ObjectId statusId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new status in the database.
	/// </summary>
	Task<Result<StatusDto>> CreateAsync(StatusDto status, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a status by its unique identifier.
	/// </summary>
	Task<Result<StatusDto>> GetByIdAsync(ObjectId statusId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all status's from the database.
	/// </summary>
	Task<Result<IReadOnlyList<StatusDto>>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated status from the database, excluding archived status by default.
	/// </summary>
	Task<Result<(IReadOnlyList<StatusDto> Items, long Total)>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing status in the database.
	/// </summary>
	Task<Result<StatusDto>> UpdateAsync(StatusDto status, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of statuses in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);

}
