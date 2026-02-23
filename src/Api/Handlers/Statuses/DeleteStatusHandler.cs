// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using Shared.Exceptions;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) statuses.
/// </summary>
public class DeleteStatusHandler
{
	private readonly IStatusRepository _repository;
	private readonly DeleteStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteStatusHandler"/> class.
	/// </summary>
	public DeleteStatusHandler(IStatusRepository repository, DeleteStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a status.
	/// </summary>
	public async Task<bool> Handle(DeleteStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Status with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetAsync(objectId);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Status with ID '{command.Id}' was not found.");

		if (getResult.Value.Archived)
			return true;

		var archiveResult = await _repository.ArchiveAsync(getResult.Value);
		return archiveResult.Success;
	}
}
