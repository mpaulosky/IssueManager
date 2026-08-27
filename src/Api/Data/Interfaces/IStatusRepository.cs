// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     IStatusRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================
namespace Api.Data.Interfaces;
public interface IStatusRepository : IRepository<StatusDto>
{
	/// <summary>
	/// Gets paginated status from the database, excluding archived status by default.
	/// </summary>
	Task<Result<(IReadOnlyList<StatusDto> Items, long Total)>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
