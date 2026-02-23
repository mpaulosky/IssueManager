// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Mappers;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for updating existing statuses.
/// </summary>
public class UpdateStatusHandler
{
	private readonly IStatusRepository _repository;
	private readonly UpdateStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateStatusHandler"/> class.
	/// </summary>
	public UpdateStatusHandler(IStatusRepository repository, UpdateStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing status.
	/// </summary>
	public async Task<StatusDto> Handle(UpdateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Status with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetAsync(objectId);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Status with ID '{command.Id}' was not found.");

		var status = getResult.Value;
		status.StatusName = command.StatusName;
		status.StatusDescription = command.StatusDescription ?? string.Empty;

		var updateResult = await _repository.UpdateAsync(objectId, status);
		if (updateResult.Failure)
			throw new NotFoundException($"Status with ID '{command.Id}' could not be updated.");

		return status.ToDto();
	}
}
