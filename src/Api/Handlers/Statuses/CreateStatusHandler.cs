// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Handler for creating new statuses.
/// </summary>
public class CreateStatusHandler
{
	/// <summary>
	/// The repository for status data access operations.
	/// </summary>
	private readonly IStatusRepository _repository;

	/// <summary>
	/// The validator for status creation commands.
	/// </summary>
	private readonly CreateStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateStatusHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for status data access operations.</param>
	/// <param name="validator">The validator for status creation commands.</param>
	public CreateStatusHandler(IStatusRepository repository, CreateStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new status.
	/// </summary>
	/// <param name="command">The command containing the status information to create.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the created status as a <see cref="StatusDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the status cannot be created in the repository.</exception>
	public async Task<StatusDto> Handle(CreateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var dto = new StatusDto(
			ObjectId.GenerateNewId(),
			command.StatusName,
			command.StatusDescription ?? string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var result = await _repository.CreateAsync(dto, cancellationToken);
		if (result.Failure)
			throw new InvalidOperationException(result.Error);

		return result.Value!;
	}
}
