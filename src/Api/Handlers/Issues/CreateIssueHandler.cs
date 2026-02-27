// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Issues;

/// <summary>
/// Handler for creating new issues.
/// </summary>
public class CreateIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly CreateIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateIssueHandler"/> class.
	/// </summary>
	public CreateIssueHandler(IIssueRepository repository, CreateIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new issue.
	/// </summary>
	public async Task<IssueDto> Handle(CreateIssueCommand command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		var model = new Issue
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty,
			DateCreated = DateTime.UtcNow,
			Status = StatusDto.Empty,
			Author = UserDto.Empty,
			Category = CategoryDto.Empty
		};

		return await _repository.CreateAsync(model.ToDto(), cancellationToken);
	}
}
