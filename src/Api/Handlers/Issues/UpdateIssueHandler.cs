// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Issues;

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
	public async Task<Result<IssueDto>> Handle(UpdateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<IssueDto>("Validation failed", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (!getResult.Success || getResult.Value is null)
			return Result.Fail<IssueDto>($"Issue with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Fail<IssueDto>($"Issue with ID '{command.Id}' is archived and cannot be updated.", ResultErrorCode.Conflict);

		var updatedIssue = getResult.Value with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty
		};

		return await _repository.UpdateAsync(updatedIssue, cancellationToken);
	}
}
