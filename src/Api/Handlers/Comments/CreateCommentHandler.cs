// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Services;

namespace Api.Handlers.Comments;

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
	/// The current user service for accessing authenticated user details.
	/// </summary>
	private readonly ICurrentUserService _currentUserService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCommentHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	/// <param name="validator">The validator for comment creation commands.</param>
	/// <param name="currentUserService">The current user service for accessing authenticated user details.</param>
	public CreateCommentHandler(ICommentRepository repository, CreateCommentValidator validator, ICurrentUserService currentUserService)
	{
		_repository = repository;
		_validator = validator;
		_currentUserService = currentUserService;
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

		var author = _currentUserService.IsAuthenticated
			? new UserDto(_currentUserService.UserId ?? string.Empty, _currentUserService.Name ?? string.Empty, _currentUserService.Email ?? string.Empty)
			: UserDto.Empty;

		var dto = new CommentDto(
			ObjectId.Empty,
			command.Title,
			command.CommentText,
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			author,
			new HashSet<string>(),
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		var result = await _repository.CreateAsync(dto, cancellationToken);
		if (result.Failure)
			throw new InvalidOperationException(result.Error);

		return result.Value!;
	}
}
