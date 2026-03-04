// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Issues;

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
	public async Task<Result<IssueDto>> Handle(UpdateIssueStatusCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<IssueDto>("Validation failed", ResultErrorCode.Validation);

		var result = await _repository.GetByIdAsync(command.IssueId, cancellationToken);
		if (!result.Success || result.Value is null)
			return Result.Fail<IssueDto>($"Issue with ID '{command.IssueId}' was not found.", ResultErrorCode.NotFound);

		var updatedIssue = result.Value with { Status = command.Status };
		return await _repository.UpdateAsync(updatedIssue, cancellationToken);
	}
}
