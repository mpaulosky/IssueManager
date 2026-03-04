// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Comments;

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
	/// <returns>A task that represents the asynchronous operation. The task result contains the result with updated comment as a <see cref="CommentDto"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the comment is not found or cannot be updated.</exception>
	public async Task<Result<CommentDto>> Handle(UpdateCommentCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<CommentDto>("Validation failed", ResultErrorCode.Validation);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<CommentDto>($"Comment with ID '{command.Id}' was not found.", ResultErrorCode.NotFound);

		var updatedComment = getResult.Value with
		{
			Title = command.Title
		};

		return await _repository.UpdateAsync(updatedComment, cancellationToken);
	}
}
