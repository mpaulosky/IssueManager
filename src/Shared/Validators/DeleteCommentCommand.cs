// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for soft-deleting (archiving) a comment.
/// </summary>
public record DeleteCommentCommand
{
	/// <summary>
	/// Gets or sets the comment ID.
	/// </summary>
	public ObjectId Id { get; init; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="DeleteCommentCommand"/>.
/// </summary>
public class DeleteCommentValidator : AbstractValidator<DeleteCommentCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteCommentValidator"/> class.
	/// </summary>
	public DeleteCommentValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Comment ID is required.");
	}
}
