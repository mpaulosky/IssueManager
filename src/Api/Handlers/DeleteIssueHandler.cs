// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using IssueManager.Shared.Validators;
using Shared.Exceptions;

namespace Api.Handlers;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) issues.
/// </summary>
public class DeleteIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly DeleteIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteIssueHandler"/> class.
	/// </summary>
	public DeleteIssueHandler(IIssueRepository repository, DeleteIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of an issue.
	/// </summary>
	public async Task<bool> Handle(DeleteIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var existingIssue = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (existingIssue is null)
			throw new NotFoundException($"Issue with ID '{command.Id}' was not found.");

		if (existingIssue.Archived)
			return true;

		return await _repository.ArchiveAsync(command.Id, cancellationToken);
	}
}
