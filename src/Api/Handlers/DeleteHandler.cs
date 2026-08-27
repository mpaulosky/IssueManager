// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers;

/// <summary>
/// Handles the soft-deletion (archiving) of any entity behind <see cref="IRepository{TDto}"/>.
/// Every entity's delete-by-archive logic was identical (validate id, get, short-circuit if
/// already archived, archive) once the repository collapse landed, so one generic handler
/// replaces a hand-written class per entity.
/// </summary>
public class DeleteHandler<TDto>
	where TDto : IArchivableDto
{
	private readonly IRepository<TDto> _repository;
	private readonly string _entityName;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteHandler{TDto}"/> class.
	/// </summary>
	public DeleteHandler(IRepository<TDto> repository, string entityName)
	{
		_repository = repository;
		_entityName = entityName;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of an entity by id.
	/// </summary>
	public async Task<Result<bool>> Handle(ObjectId id, CancellationToken cancellationToken = default)
	{
		if (id == ObjectId.Empty)
			return Result.Fail<bool>($"{_entityName} ID cannot be empty.", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<bool>($"{_entityName} with ID '{id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Ok(true);

		var archiveResult = await _repository.ArchiveAsync(id, cancellationToken);
		return archiveResult.Success ? Result.Ok(true) : Result.Fail<bool>(archiveResult.Error!, archiveResult.ErrorCode);
	}
}
