// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using Api.Data;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;
using Shared.Validators;

namespace Api.Handlers;

/// <summary>
/// Handler for creating new categories.
/// </summary>
public class CreateCategoryHandler
{
	private readonly ICategoryRepository _repository;
	private readonly CreateCategoryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCategoryHandler"/> class.
	/// </summary>
	public CreateCategoryHandler(ICategoryRepository repository, CreateCategoryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new category.
	/// </summary>
	public async Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var model = new Category
		{
			CategoryName = command.CategoryName,
			CategoryDescription = command.CategoryDescription ?? string.Empty,
			DateCreated = DateTime.UtcNow
		};

		var result = await _repository.CreateAsync(model);
		if (result.Failure)
			throw new InvalidOperationException(result.Error);

		return model.ToDto();
	}
}
