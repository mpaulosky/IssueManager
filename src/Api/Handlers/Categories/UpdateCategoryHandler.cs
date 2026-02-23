// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;

using FluentValidation;

using Shared.DTOs;
using Shared.Exceptions;
using Shared.Mappers;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for updating existing categories.
/// </summary>
public class UpdateCategoryHandler
{
	/// <summary>
	/// The repository for category data access operations.
	/// </summary>
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// The validator for category update commands.
	/// </summary>
	private readonly UpdateCategoryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateCategoryHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for category data access operations.</param>
	/// <param name="validator">The validator for category update commands.</param>
	public UpdateCategoryHandler(ICategoryRepository repository, UpdateCategoryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing category.
	/// </summary>
	/// <param name="command">The command containing the updated category information.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the updated category as a <see cref="CategoryDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the category is not found or cannot be updated.</exception>
	public async Task<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Category with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetAsync(objectId);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Category with ID '{command.Id}' was not found.");

		var category = getResult.Value;
		category.CategoryName = command.CategoryName;
		category.CategoryDescription = command.CategoryDescription ?? string.Empty;

		var updateResult = await _repository.UpdateAsync(objectId, category);
		if (updateResult.Failure)
			throw new NotFoundException($"Category with ID '{command.Id}' could not be updated.");

		return category.ToDto();
	}
}
