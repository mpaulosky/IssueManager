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
	public async Task<Result<bool>> Handle(DeleteIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<bool>("Validation failed", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<bool>($"Issue with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Ok(true);

		var archiveResult = await _repository.ArchiveAsync(command.Id, cancellationToken);
		return archiveResult.Success ? Result.Ok(true) : Result.Fail<bool>(archiveResult.Error!, archiveResult.ErrorCode);
	}
}
