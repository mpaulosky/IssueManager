// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for soft-deleting (archiving) a status.
/// </summary>
public record DeleteStatusCommand
{
	/// <summary>
	/// Gets or sets the status ID.
	/// </summary>
	public string Id { get; init; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="DeleteStatusCommand"/>.
/// </summary>
public class DeleteStatusValidator : AbstractValidator<DeleteStatusCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteStatusValidator"/> class.
	/// </summary>
	public DeleteStatusValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Status ID is required.");
	}
}
