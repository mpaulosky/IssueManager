// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentHandler.cs
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
/// Handler for updating existing comments.
/// </summary>
public class UpdateCommentHandler
{
	/// <summary>
	/// The repository for comment data access operations.
	/// </summary>
	private readonly ICommentRepository _repository;

	/// <summary>
	/// The validator for comment update commands.
	/// </summary>
	private readonly UpdateCommentValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateCommentHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	/// <param name="validator">The validator for comment update commands.</param>
	public UpdateCommentHandler(ICommentRepository repository, UpdateCommentValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing comment.
	/// </summary>
	/// <param name="command">The command containing the updated comment information.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the updated comment as a <see cref="CommentDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the comment is not found or cannot be updated.</exception>
	public async Task<CommentDto> Handle(UpdateCommentCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		if (!MongoDB.Bson.ObjectId.TryParse(command.Id, out var objectId))
			throw new NotFoundException($"Comment with ID '{command.Id}' was not found.");

		var getResult = await _repository.GetAsync(objectId);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Comment with ID '{command.Id}' was not found.");

		var comment = getResult.Value;
		comment.Title = command.Title;

		var updateResult = await _repository.UpdateAsync(objectId, comment);
		if (updateResult.Failure)
			throw new NotFoundException($"Comment with ID '{command.Id}' could not be updated.");

		return comment.ToDto();
	}
}
