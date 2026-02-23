// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListIssuesHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using FluentValidation;
using IssueManager.Api.Data;
using IssueManager.Shared.Validators;
using Shared.DTOs;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for listing issues with pagination.
/// </summary>
public class ListIssuesHandler
{
	private readonly IIssueRepository _repository;
	private readonly ListIssuesQueryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListIssuesHandler"/> class.
	/// </summary>
	public ListIssuesHandler(IIssueRepository repository, ListIssuesQueryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the retrieval of a paginated list of issues.
	/// </summary>
	public async Task<PaginatedResponse<IssueDto>> Handle(ListIssuesQuery query, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(query, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var (items, total) = await _repository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

		return new PaginatedResponse<IssueDto>(items, total, query.Page, query.PageSize);
	}
}
