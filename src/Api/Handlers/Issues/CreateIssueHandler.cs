// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data.Interfaces;
using Api.Services;

namespace Api.Handlers.Issues;

/// <summary>
/// Handler for creating new issues.
/// </summary>
public class CreateIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly ICategoryRepository _categoryRepository;
	private readonly IStatusRepository _statusRepository;
	private readonly CreateIssueValidator _validator;
	private readonly ICurrentUserService _currentUserService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateIssueHandler"/> class.
	/// </summary>
	public CreateIssueHandler(
		IIssueRepository repository,
		ICategoryRepository categoryRepository,
		IStatusRepository statusRepository,
		CreateIssueValidator validator,
		ICurrentUserService currentUserService)
	{
		_repository = repository;
		_categoryRepository = categoryRepository;
		_statusRepository = statusRepository;
		_validator = validator;
		_currentUserService = currentUserService;
	}

	/// <summary>
	/// Handles the creation of a new issue.
	/// </summary>
	public async Task<Result<IssueDto>> Handle(CreateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<IssueDto>("Validation failed: " + string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), ResultErrorCode.Validation);

		var author = _currentUserService.IsAuthenticated
			? new UserDto(_currentUserService.UserId ?? string.Empty, _currentUserService.Name ?? string.Empty, _currentUserService.Email ?? string.Empty)
			: UserDto.Empty;

		var category = await LookupByIdAsync(command.CategoryId, CategoryDto.Empty, _categoryRepository.GetByIdAsync, cancellationToken);
		var status = await LookupByIdAsync(command.StatusId, StatusDto.Empty, _statusRepository.GetByIdAsync, cancellationToken);

		var model = new Issue
		{
			Id = ObjectId.GenerateNewId(),
			Title = command.Title,
			Description = command.Description ?? string.Empty,
			DateCreated = DateTime.UtcNow,
			Status = status,
			Author = author,
			Category = category
		};

		var result = await _repository.CreateAsync(model.ToDto(), cancellationToken);
		if (!result.Success)
			return Result.Fail<IssueDto>(result.Error ?? "Failed to create issue.");

		return Result.Ok(result.Value!);
	}

	private static async Task<T> LookupByIdAsync<T>(
		string? id,
		T defaultValue,
		Func<ObjectId, CancellationToken, Task<Result<T>>> getById,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrEmpty(id) && ObjectId.TryParse(id, out var objectId))
		{
			var result = await getById(objectId, cancellationToken);
			if (result.Success && result.Value is not null)
				return result.Value;
		}
		return defaultValue;
	}
}
