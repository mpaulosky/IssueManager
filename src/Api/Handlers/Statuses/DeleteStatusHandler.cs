// =============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) statuses.
/// </summary>
public class DeleteStatusHandler
{
	private readonly IStatusRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteStatusHandler"/> class.
	/// </summary>
	public DeleteStatusHandler(IStatusRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a status.
	/// </summary>
	public async Task<Result<bool>> Handle(DeleteStatusCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Id == ObjectId.Empty)
			return Result.Fail<bool>("Status ID cannot be empty.", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<bool>($"Status with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Ok(true);

		var archiveResult = await _repository.ArchiveAsync(command.Id, cancellationToken);
		return archiveResult.Success ? Result.Ok(true) : Result.Fail<bool>(archiveResult.Error!, archiveResult.ErrorCode);
	}
}
