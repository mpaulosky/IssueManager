// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;

using FluentValidation;

using Shared.Exceptions;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) categories.
/// </summary>
public class DeleteCategoryHandler
{
	/// <summary>
	/// The repository for category data access operations.
	/// </summary>
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// The validator for category deletion commands.
	/// </summary>
	private readonly DeleteCategoryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteCategoryHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for category data access operations.</param>
	/// <param name="validator">The validator for category deletion commands.</param>
	public DeleteCategoryHandler(ICategoryRepository repository, DeleteCategoryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a category.
	/// </summary>
	/// <param name="command">The command containing the category ID to delete.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns><see langword="true"/> if the category was successfully archived; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the category is not found.</exception>
	public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Category with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetAsync(objectId);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Category with ID '{command.Id}' was not found.");

		if (getResult.Value.Archived)
			return true;

		var archiveResult = await _repository.ArchiveAsync(getResult.Value);
		return archiveResult.Success;
	}
}
