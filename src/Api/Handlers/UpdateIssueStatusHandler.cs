// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using IssueManager.Api.Data;
using Shared.DTOs;
using Shared.Validators;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for updating issue status.
/// </summary>
public class UpdateIssueStatusHandler
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueStatusHandler"/> class.
	/// </summary>
	public UpdateIssueStatusHandler(IIssueRepository repository, UpdateIssueStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of issue status.
	/// </summary>
	public async Task<IssueDto?> Handle(UpdateIssueStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var existingIssue = await _repository.GetByIdAsync(command.IssueId, cancellationToken);
		if (existingIssue is null)
			return null;

		var updatedIssue = existingIssue with { Status = command.Status };
		return await _repository.UpdateAsync(updatedIssue, cancellationToken);
	}
}
