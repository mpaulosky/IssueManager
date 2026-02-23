// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for creating new statuses.
/// </summary>
public class CreateStatusHandler
{
	private readonly IStatusRepository _repository;
	private readonly CreateStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateStatusHandler"/> class.
	/// </summary>
	public CreateStatusHandler(IStatusRepository repository, CreateStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new status.
	/// </summary>
	public async Task<StatusDto> Handle(CreateStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var model = new Status
		{
			StatusName = command.StatusName,
			StatusDescription = command.StatusDescription ?? string.Empty,
			DateCreated = DateTime.UtcNow
		};

		var result = await _repository.CreateAsync(model);
		if (result.Failure)
			throw new InvalidOperationException(result.Error);

		return model.ToDto();
	}
}
