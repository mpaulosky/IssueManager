// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
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
	private readonly ICategoryRepository _repository;
	private readonly UpdateCategoryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateCategoryHandler"/> class.
	/// </summary>
	public UpdateCategoryHandler(ICategoryRepository repository, UpdateCategoryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing category.
	/// </summary>
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
