// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for updating an existing comment.
/// </summary>
public record UpdateCommentCommand
{
	/// <summary>
	/// Gets or sets the comment ID.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	/// Gets or sets the title of the comment.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the text content of the comment.
	/// </summary>
	public string CommentText { get; init; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="UpdateCommentCommand"/>.
/// </summary>
public class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateCommentValidator"/> class.
	/// </summary>
	public UpdateCommentValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Comment ID is required.");

		RuleFor(x => x.CommentText)
			.NotEmpty()
			.WithMessage("Comment text is required.")
			.MinimumLength(1)
			.WithMessage("Comment text must be at least 1 character long.")
			.MaximumLength(5000)
			.WithMessage("Comment text cannot exceed 5000 characters.");
	}
}
