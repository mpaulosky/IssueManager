// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using IssueManager.Shared.Validators;
using Shared.DTOs;
using Shared.Exceptions;

namespace Api.Handlers;

/// <summary>
/// Handler for updating existing issues.
/// </summary>
public class UpdateIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueHandler"/> class.
	/// </summary>
	public UpdateIssueHandler(IIssueRepository repository, UpdateIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing issue.
	/// </summary>
	public async Task<IssueDto> Handle(UpdateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var existingIssue = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (existingIssue is null)
			throw new NotFoundException($"Issue with ID '{command.Id}' was not found.");

		if (existingIssue.Archived)
			throw new ConflictException($"Issue with ID '{command.Id}' is archived and cannot be updated.");

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty
		};

		var result = await _repository.UpdateAsync(updatedIssue, cancellationToken);
		if (result is null)
			throw new NotFoundException($"Issue with ID '{command.Id}' could not be updated.");

		return result;
	}
}
