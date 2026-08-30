// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     TaxonomyCrudHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers;

/// <summary>
/// Bridges a Taxonomy entity's entity-prefixed fields (e.g. CategoryName, StatusName) to the
/// generic shape <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/> operates on,
/// without renaming the DTO/command properties that make up the wire contract.
/// </summary>
public sealed class TaxonomyAdapter<TDto, TCreateCmd, TUpdateCmd>
	where TDto : IArchivableDto
{
	/// <summary>The display name used in validation and not-found error messages.</summary>
	public required string EntityName { get; init; }

	public required Func<TCreateCmd, string> GetCreateName { get; init; }

	public required Func<TCreateCmd, string?> GetCreateDescription { get; init; }

	public required Func<TUpdateCmd, ObjectId> GetUpdateId { get; init; }

	public required Func<TUpdateCmd, string> GetUpdateName { get; init; }

	public required Func<TUpdateCmd, string?> GetUpdateDescription { get; init; }

	/// <summary>Builds a new DTO for a create operation from its generated id, name, description and creation timestamp.</summary>
	public required Func<ObjectId, string, string, DateTime, TDto> NewDto { get; init; }

	/// <summary>Returns a copy of an existing DTO with its name and description replaced.</summary>
	public required Func<TDto, string, string, TDto> WithNameDescription { get; init; }
}

/// <summary>
/// Validates the name/description shape shared by every Taxonomy entity's create and update
/// commands, reading the fields through the delegates an adapter supplies rather than a fixed
/// property name.
/// </summary>
public sealed class TaxonomyCommandValidator<TCommand> : AbstractValidator<TCommand>
{
	public TaxonomyCommandValidator(
		string entityName,
		Func<TCommand, string> getName,
		Func<TCommand, string?> getDescription,
		Func<TCommand, ObjectId>? getId = null)
	{
		RuleFor(x => x).Custom((command, context) =>
		{
			if (getId is not null && getId(command) == ObjectId.Empty)
				context.AddFailure("Id", $"{entityName} ID is required.");

			var name = getName(command);
			if (string.IsNullOrEmpty(name))
			{
				context.AddFailure("Name", $"{entityName} name is required.");
			}
			else
			{
				if (name.Length < 2)
					context.AddFailure("Name", $"{entityName} name must be at least 2 characters long.");

				if (name.Length > 100)
					context.AddFailure("Name", $"{entityName} name cannot exceed 100 characters.");
			}

			var description = getDescription(command);
			if (!string.IsNullOrEmpty(description) && description.Length > 500)
				context.AddFailure("Description", $"{entityName} description cannot exceed 500 characters.");
		});
	}
}

/// <summary>
/// Handles create, update and list for any Taxonomy entity (a named, described reference entity
/// that classifies an Issue - see CONTEXT.md). Category and Status share this one implementation
/// via a <see cref="TaxonomyAdapter{TDto,TCreateCmd,TUpdateCmd}"/> supplying their entity-specific
/// field access; get-by-id has no behavior worth generalizing and is handled by endpoints calling
/// the repository directly.
/// </summary>
public sealed class TaxonomyCrudHandler<TDto, TCreateCmd, TUpdateCmd>
	where TDto : IArchivableDto
{
	private readonly IRepository<TDto> _repository;

	private readonly TaxonomyAdapter<TDto, TCreateCmd, TUpdateCmd> _adapter;

	private readonly TaxonomyCommandValidator<TCreateCmd> _createValidator;

	private readonly TaxonomyCommandValidator<TUpdateCmd> _updateValidator;

	public TaxonomyCrudHandler(IRepository<TDto> repository, TaxonomyAdapter<TDto, TCreateCmd, TUpdateCmd> adapter)
	{
		_repository = repository;
		_adapter = adapter;
		_createValidator = new TaxonomyCommandValidator<TCreateCmd>(adapter.EntityName, adapter.GetCreateName, adapter.GetCreateDescription);
		_updateValidator = new TaxonomyCommandValidator<TUpdateCmd>(adapter.EntityName, adapter.GetUpdateName, adapter.GetUpdateDescription, adapter.GetUpdateId);
	}

	/// <summary>Handles the creation of a new Taxonomy entity.</summary>
	public async Task<Result<TDto>> HandleCreate(TCreateCmd command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<TDto>("Validation failed: " + string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), ResultErrorCode.Validation);

		var dto = _adapter.NewDto(
			ObjectId.GenerateNewId(),
			_adapter.GetCreateName(command),
			_adapter.GetCreateDescription(command) ?? string.Empty,
			DateTime.UtcNow);

		return await _repository.CreateAsync(dto, cancellationToken);
	}

	/// <summary>Handles the update of an existing Taxonomy entity.</summary>
	public async Task<Result<TDto>> HandleUpdate(TUpdateCmd command, CancellationToken cancellationToken = default)
	{
		var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
			return Result.Fail<TDto>("Validation failed", ResultErrorCode.Validation);

		var id = _adapter.GetUpdateId(command);

		var getResult = await _repository.GetByIdAsync(id, cancellationToken);
		if (getResult.Failure || getResult.Value is null)
			return Result.Fail<TDto>($"{_adapter.EntityName} with ID '{id}' was not found.", ResultErrorCode.NotFound);

		var updated = _adapter.WithNameDescription(
			getResult.Value,
			_adapter.GetUpdateName(command),
			_adapter.GetUpdateDescription(command) ?? string.Empty);

		return await _repository.UpdateAsync(updated, cancellationToken);
	}

	/// <summary>Handles the retrieval of all Taxonomy entities of this kind.</summary>
	public async Task<IEnumerable<TDto>> HandleList(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync(cancellationToken);
		if (!result.Success)
			return Enumerable.Empty<TDto>();

		return result.Value ?? Enumerable.Empty<TDto>();
	}
}
