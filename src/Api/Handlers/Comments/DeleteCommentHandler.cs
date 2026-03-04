// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Comments;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) comments.
/// </summary>
public class DeleteCommentHandler
{
	/// <summary>
	/// The repository for comment data access operations.
	/// </summary>
	private readonly ICommentRepository _repository;

	/// <summary>
	/// The validator for comment deletion commands.
	/// </summary>
	private readonly DeleteCommentValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteCommentHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	/// <param name="validator">The validator for comment deletion commands.</param>
	public DeleteCommentHandler(ICommentRepository repository, DeleteCommentValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of a comment.
	/// </summary>
	/// <param name="command">The command containing the comment ID to delete.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns><see langword="true"/> if the comment was successfully archived; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
	/// <exception cref="NotFoundException">Thrown when the comment is not found.</exception>
	public async Task<bool> Handle(DeleteCommentCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var getResult = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			throw new NotFoundException($"Comment with ID '{command.Id}' was not found.");

		if (getResult.Value.Archived)
			return true;

		var archiveResult = await _repository.ArchiveAsync(command.Id, cancellationToken);
		return archiveResult.Success;
	}
}
