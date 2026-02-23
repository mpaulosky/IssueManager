// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentHandler.cs
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
/// Handler for creating new comments.
/// </summary>
public class CreateCommentHandler
{
	private readonly ICommentRepository _repository;
	private readonly CreateCommentValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCommentHandler"/> class.
	/// </summary>
	public CreateCommentHandler(ICommentRepository repository, CreateCommentValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new comment.
	/// </summary>
	public async Task<CommentDto> Handle(CreateCommentCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var model = new Comment
		{
			Title = command.Title,
			Description = command.CommentText,
			DateCreated = DateTime.UtcNow
		};

		var result = await _repository.CreateAsync(model);
		if (result.Failure)
			throw new InvalidOperationException(result.Error);

		return model.ToDto();
	}
}
