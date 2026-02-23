// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;

using FluentValidation;

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
	/// <summary>
	/// The repository for comment data access operations.
	/// </summary>
	private readonly ICommentRepository _repository;

	/// <summary>
	/// The validator for comment creation commands.
	/// </summary>
	private readonly CreateCommentValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCommentHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	/// <param name="validator">The validator for comment creation commands.</param>
	public CreateCommentHandler(ICommentRepository repository, CreateCommentValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new comment.
	/// </summary>
	/// <param name="command">The command containing the comment information to create.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the created comment as a <see cref="CommentDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the comment cannot be created in the repository.</exception>
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
