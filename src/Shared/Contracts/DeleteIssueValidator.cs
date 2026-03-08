// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
using FluentValidation;

namespace Shared.Contracts;

/// <summary>
/// Validates the DeleteIssueCommand.
/// </summary>
public class DeleteIssueValidator : AbstractValidator<DeleteIssueCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteIssueValidator"/> class.
	/// </summary>
	public DeleteIssueValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Issue ID is required.");
	}
}
