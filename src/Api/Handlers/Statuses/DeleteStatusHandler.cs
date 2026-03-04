// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) statuses.
/// </summary>
public class DeleteStatusHandler
{
	/// <summary>
	/// The repository for status data access operations.
	/// </summary>
	private readonly IStatusRepository _repository;

	/// <summary>
	/// The validator for status deletion commands.
	/// </summary>
	private readonly DeleteStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteStatusHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for status data access operations.</param>
	/// <param name="validator">The validator for status deletion commands.</param>
	public DeleteStatusHandler(IStatusRepository repository, DeleteStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a status.
	/// </summary>
	/// <param name="command">The command containing the status ID to delete.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a result indicating success or failure.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the status is not found.</exception>
	public async Task<Result<bool>> Handle(DeleteStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<bool>("Validation failed", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<bool>($"Status with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Ok(true);

		var archiveResult = await _repository.ArchiveAsync(command.Id, cancellationToken);
		return archiveResult.Success ? Result.Ok(true) : Result.Fail<bool>(archiveResult.Error!, archiveResult.ErrorCode);
	}
}
