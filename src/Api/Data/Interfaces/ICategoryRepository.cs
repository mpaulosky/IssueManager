// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     ICategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================
namespace Api.Data.Interfaces;
public interface ICategoryRepository : IRepository<CategoryDto>
{
	/// <summary>
	/// Gets paginated categories from the database, excluding archived categories by default.
	/// </summary>
	Task<Result<(IReadOnlyList<CategoryDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default);
}
