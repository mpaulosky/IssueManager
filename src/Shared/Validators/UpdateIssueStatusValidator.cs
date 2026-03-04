// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     UpdateIssueStatusValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for updating the status of an issue.
/// </summary>
public record UpdateIssueStatusCommand
{
	/// <summary>
	/// Gets or sets the ID of the issue to update.
	/// </summary>
	public ObjectId IssueId { get; init; }

	/// <summary>
	/// Gets or sets the new status for the issue.
	/// </summary>
	public StatusDto Status { get; init; } = StatusDto.Empty;
}

/// <summary>
/// Validates the UpdateIssueStatusCommand.
/// </summary>
public class UpdateIssueStatusValidator : AbstractValidator<UpdateIssueStatusCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueStatusValidator"/> class.
	/// </summary>
	public UpdateIssueStatusValidator()
	{
		RuleFor(x => x.IssueId)
			.NotEmpty()
			.WithMessage("Issue ID is required.");

		RuleFor(x => x.Status.StatusName)
			.NotEmpty()
			.WithMessage("Status name is required.");
	}
}
