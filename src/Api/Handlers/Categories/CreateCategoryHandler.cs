// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Categories;

/// <summary>
/// Handler for creating new categories.
/// </summary>
public class CreateCategoryHandler
{
	/// <summary>
	/// The repository for category data access operations.
	/// </summary>
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// The validator for category creation commands.
	/// </summary>
	private readonly CreateCategoryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCategoryHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for category data access operations.</param>
	/// <param name="validator">The validator for category creation commands.</param>
	public CreateCategoryHandler(ICategoryRepository repository, CreateCategoryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new category.
	/// </summary>
	/// <param name="command">The command containing the category information to create.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> wrapping the created category as a <see cref="CategoryDto"/>, or a validation/repository failure.</returns>
	public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<CategoryDto>("Validation failed: " + string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), ResultErrorCode.Validation);

		var dto = new CategoryDto(
			ObjectId.GenerateNewId(),
			command.CategoryName,
			command.CategoryDescription ?? string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		return await _repository.CreateAsync(dto, cancellationToken);
	}
}
