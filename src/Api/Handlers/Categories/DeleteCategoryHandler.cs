// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Categories;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) categories.
/// </summary>
public class DeleteCategoryHandler
{
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteCategoryHandler"/> class.
	/// </summary>
	public DeleteCategoryHandler(ICategoryRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a category.
	/// </summary>
	public async Task<Result<bool>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
	{
		if (command.Id == ObjectId.Empty)
			return Result.Fail<bool>("Category ID cannot be empty.", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<bool>($"Category with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		if (getResult.Value.Archived)
			return Result.Ok(true);

		var archiveResult = await _repository.ArchiveAsync(command.Id, cancellationToken);
		return archiveResult.Success ? Result.Ok(true) : Result.Fail<bool>(archiveResult.Error!, archiveResult.ErrorCode);
	}
}
