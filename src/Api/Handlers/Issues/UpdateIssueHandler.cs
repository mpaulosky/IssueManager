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
	private readonly ICategoryRepository _categoryRepository;
	private readonly IStatusRepository _statusRepository;
	private readonly UpdateIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueHandler"/> class.
	/// </summary>
	public UpdateIssueHandler(
		IIssueRepository repository,
		ICategoryRepository categoryRepository,
		IStatusRepository statusRepository,
		UpdateIssueValidator validator)
	{
		_repository = repository;
		_categoryRepository = categoryRepository;
		_statusRepository = statusRepository;
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

		var category = getResult.Value.Category;
		if (command.CategoryId.HasValue && command.CategoryId.Value != ObjectId.Empty)
		{
			var categoryResult = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken);
			if (!categoryResult.Success || categoryResult.Value is null)
				return Result.Fail<IssueDto>($"Category with ID '{command.CategoryId}' was not found.", ResultErrorCode.NotFound);
			category = categoryResult.Value;
		}

		var status = getResult.Value.Status;
		if (command.StatusId.HasValue && command.StatusId.Value != ObjectId.Empty)
		{
			var statusResult = await _statusRepository.GetByIdAsync(command.StatusId.Value, cancellationToken);
			if (!statusResult.Success || statusResult.Value is null)
				return Result.Fail<IssueDto>($"Status with ID '{command.StatusId}' was not found.", ResultErrorCode.NotFound);
			status = statusResult.Value;
		}

		var updatedIssue = getResult.Value with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty,
			Category = category,
			Status = status,
			ApprovedForRelease = command.ApprovedForRelease ?? getResult.Value.ApprovedForRelease,
			Rejected = command.Rejected ?? getResult.Value.Rejected
		};

		return await _repository.UpdateAsync(updatedIssue, cancellationToken);
	}
}
