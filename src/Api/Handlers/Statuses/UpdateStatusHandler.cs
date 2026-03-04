// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Handler for updating existing statuses.
/// </summary>
public class UpdateStatusHandler
{
	/// <summary>
	/// The repository for status data access operations.
	/// </summary>
	private readonly IStatusRepository _repository;

	/// <summary>
	/// The validator for status update commands.
	/// </summary>
	private readonly UpdateStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateStatusHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for status data access operations.</param>
	/// <param name="validator">The validator for status update commands.</param>
	public UpdateStatusHandler(IStatusRepository repository, UpdateStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing status.
	/// </summary>
	/// <param name="command">The command containing the updated status information.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the result with updated status as a <see cref="StatusDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the status is not found or cannot be updated.</exception>
	public async Task<Result<StatusDto>> Handle(UpdateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<StatusDto>("Validation failed", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<StatusDto>($"Status with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		var updatedStatus = getResult.Value with
		{
			StatusName = command.StatusName,
			StatusDescription = command.StatusDescription ?? string.Empty
		};

		return await _repository.UpdateAsync(updatedStatus, cancellationToken);
	}
}
