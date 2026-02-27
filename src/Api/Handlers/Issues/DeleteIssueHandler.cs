// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Issues;

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

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Issue with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetByIdAsync(objectId, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Issue with ID '{command.Id}' was not found.");

		if (getResult.Value.Archived)
			return true;

		var archiveResult = await _repository.ArchiveAsync(objectId, cancellationToken);
		return archiveResult.Success;
	}
}
