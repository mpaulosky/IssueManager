// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Contracts;

/// <summary>
/// Command for creating a new comment.
/// </summary>
public record CreateCommentCommand
{
	/// <summary>
	/// Gets or sets the title of the comment.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the text content of the comment.
	/// </summary>
	public string CommentText { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the ID of the issue this comment belongs to.
	/// </summary>
	public string IssueId { get; init; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="CreateCommentCommand"/>.
/// </summary>
public class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CreateCommentValidator"/> class.
	/// </summary>
	public CreateCommentValidator()
	{
		RuleFor(x => x.CommentText)
			.NotEmpty()
			.WithMessage("Comment text is required.")
			.MinimumLength(1)
			.WithMessage("Comment text must be at least 1 character long.")
			.MaximumLength(5000)
			.WithMessage("Comment text cannot exceed 5000 characters.");

		RuleFor(x => x.IssueId)
			.NotEmpty()
			.WithMessage("Issue ID is required.")
			.Must(id => ObjectId.TryParse(id, out _))
			.WithMessage("Issue ID must be a valid ObjectId.");
	}
}
