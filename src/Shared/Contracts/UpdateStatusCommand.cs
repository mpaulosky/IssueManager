// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Contracts;

/// <summary>
/// Command for updating an existing status.
/// </summary>
public record UpdateStatusCommand
{
	/// <summary>
	/// Gets or sets the status ID.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	/// Gets or sets the name of the status.
	/// </summary>
	public string StatusName { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the status.
	/// </summary>
	public string? StatusDescription { get; init; }
}

/// <summary>
/// Validates the <see cref="UpdateStatusCommand"/>.
/// </summary>
public class UpdateStatusValidator : AbstractValidator<UpdateStatusCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateStatusValidator"/> class.
	/// </summary>
	public UpdateStatusValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Status ID is required.");

		RuleFor(x => x.StatusName)
			.NotEmpty()
			.WithMessage("Status name is required.")
			.MinimumLength(2)
			.WithMessage("Status name must be at least 2 characters long.")
			.MaximumLength(100)
			.WithMessage("Status name cannot exceed 100 characters.");

		RuleFor(x => x.StatusDescription)
			.MaximumLength(500)
			.WithMessage("Status description cannot exceed 500 characters.")
			.When(x => !string.IsNullOrEmpty(x.StatusDescription));
	}
}
