// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Categories;

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

		var getResult = await _repository.GetByIdAsync(objectId, cancellationToken);
		if (!getResult.Success || getResult.Value is null)
			throw new NotFoundException($"Category with ID '{command.Id}' was not found.");

		var updatedCategory = getResult.Value with
		{
			CategoryName = command.CategoryName,
			CategoryDescription = command.CategoryDescription ?? string.Empty
		};

		var updateResult = await _repository.UpdateAsync(updatedCategory, cancellationToken);
		if (!updateResult.Success)
			throw new NotFoundException($"Category with ID '{command.Id}' could not be updated.");

		return updateResult.Value;
	}
}
