// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     TaxonomyCrudHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers;

/// <summary>
/// Unit tests for <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/>, exercised against
/// a synthetic Widget entity so the tests verify the generic behavior itself rather than any one
/// real Taxonomy entity. Category and Status each get a thin smoke test instead (see their own
/// Handlers folders) confirming their adapter is wired correctly.
/// </summary>
[ExcludeFromCodeCoverage]
public class TaxonomyCrudHandlerTests
{
	// Public (not private) because NSubstitute proxies IRepository<WidgetDto> across the assembly
	// boundary and Castle DynamicProxy cannot build a proxy over an inaccessible type argument.
	public sealed record WidgetDto(ObjectId Id, string Name, string Description, DateTime DateCreated, DateTime? DateModified, bool Archived, UserDto ArchivedBy) : IArchivableDto;

	public sealed record CreateWidgetCommand(string Name, string? Description);

	public sealed record UpdateWidgetCommand(ObjectId Id, string Name, string? Description);

	private static readonly TaxonomyAdapter<WidgetDto, CreateWidgetCommand, UpdateWidgetCommand> Adapter = new()
	{
		EntityName = "Widget",
		GetCreateName = c => c.Name,
		GetCreateDescription = c => c.Description,
		GetUpdateId = c => c.Id,
		GetUpdateName = c => c.Name,
		GetUpdateDescription = c => c.Description,
		NewDto = (id, name, description, createdAt) => new WidgetDto(id, name, description, createdAt, null, false, UserDto.Empty),
		WithNameDescription = (dto, name, description) => dto with { Name = name, Description = description }
	};

	private readonly IRepository<WidgetDto> _repository;
	private readonly TaxonomyCrudHandler<WidgetDto, CreateWidgetCommand, UpdateWidgetCommand> _handler;

	public TaxonomyCrudHandlerTests()
	{
		_repository = Substitute.For<IRepository<WidgetDto>>();
		_handler = new TaxonomyCrudHandler<WidgetDto, CreateWidgetCommand, UpdateWidgetCommand>(_repository, Adapter);
	}

	// ── HandleCreate ───────────────────────────────────────────────────────

	[Fact]
	public async Task HandleCreate_ValidCommand_ReturnsCreatedEntity()
	{
		var command = new CreateWidgetCommand("Bug", "Bug reports");

		_repository.CreateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<WidgetDto>.Ok(callInfo.Arg<WidgetDto>()));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.Name.Should().Be("Bug");
		result.Value!.Description.Should().Be("Bug reports");
		await _repository.Received(1).CreateAsync(Arg.Is<WidgetDto>(d => d.Name == "Bug" && d.Description == "Bug reports"), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleCreate_EmptyName_ReturnsValidationFailure()
	{
		var command = new CreateWidgetCommand("", "Description");

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Widget name").And.Contain("required");
	}

	[Fact]
	public async Task HandleCreate_NameTooShort_ReturnsValidationFailure()
	{
		var command = new CreateWidgetCommand("A", "Description");

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.Error.Should().Contain("at least 2 characters");
	}

	[Fact]
	public async Task HandleCreate_NameTooLong_ReturnsValidationFailure()
	{
		var command = new CreateWidgetCommand(new string('A', 101), "Description");

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.Error.Should().Contain("cannot exceed 100 characters");
	}

	[Fact]
	public async Task HandleCreate_DescriptionTooLong_ReturnsValidationFailure()
	{
		var command = new CreateWidgetCommand("Valid Name", new string('X', 501));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.Error.Should().Contain("cannot exceed 500 characters");
	}

	[Fact]
	public async Task HandleCreate_NullDescription_UsesEmptyString()
	{
		var command = new CreateWidgetCommand("Valid Name", null);

		_repository.CreateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<WidgetDto>.Ok(callInfo.Arg<WidgetDto>()));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Value!.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleCreate_RepositoryFails_ReturnsFailureResult()
	{
		var command = new CreateWidgetCommand("Valid Name", "Description");

		_repository.CreateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<WidgetDto>.Fail("Database error"));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	// ── HandleUpdate ───────────────────────────────────────────────────────

	[Fact]
	public async Task HandleUpdate_ValidCommand_ReturnsUpdatedEntity()
	{
		var id = ObjectId.GenerateNewId();
		var existing = new WidgetDto(id, "Old Name", "Old Description", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateWidgetCommand(id, "Updated Name", "Updated Description");

		_repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(Result<WidgetDto>.Ok(existing));
		_repository.UpdateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<WidgetDto>.Ok(callInfo.Arg<WidgetDto>()));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.Name.Should().Be("Updated Name");
		result.Value!.Description.Should().Be("Updated Description");
		await _repository.Received(1).UpdateAsync(Arg.Is<WidgetDto>(d => d.Name == "Updated Name" && d.Description == "Updated Description"), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleUpdate_EmptyName_ReturnsValidationFailure()
	{
		var command = new UpdateWidgetCommand(ObjectId.GenerateNewId(), "", "Description");

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task HandleUpdate_EmptyId_ReturnsValidationFailure()
	{
		var command = new UpdateWidgetCommand(ObjectId.Empty, "Valid Name", "Description");

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task HandleUpdate_NonExistentEntity_ReturnsNotFoundResult()
	{
		var id = ObjectId.GenerateNewId();
		var command = new UpdateWidgetCommand(id, "Updated Name", "Updated Description");

		_repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(Result<WidgetDto>.Fail("Not found"));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task HandleUpdate_RepositoryUpdateFails_ReturnsFailResult()
	{
		var id = ObjectId.GenerateNewId();
		var existing = new WidgetDto(id, "Old Name", "", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateWidgetCommand(id, "Updated Name", "Updated Description");

		_repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(Result<WidgetDto>.Ok(existing));
		_repository.UpdateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>()).Returns(Result<WidgetDto>.Fail("Update failed"));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeFalse();
		result.Error.Should().Be("Update failed");
	}

	[Fact]
	public async Task HandleUpdate_NullDescription_UsesEmptyString()
	{
		var id = ObjectId.GenerateNewId();
		var existing = new WidgetDto(id, "Old Name", "", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateWidgetCommand(id, "Updated Name", null);

		_repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(Result<WidgetDto>.Ok(existing));
		_repository.UpdateAsync(Arg.Any<WidgetDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<WidgetDto>.Ok(callInfo.Arg<WidgetDto>()));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.Description.Should().BeEmpty();
	}

	// ── HandleList ─────────────────────────────────────────────────────────

	[Fact]
	public async Task HandleList_ReturnsAllEntities()
	{
		IReadOnlyList<WidgetDto> widgets =
		[
			new(ObjectId.GenerateNewId(), "Bug", "", DateTime.UtcNow, null, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Feature", "", DateTime.UtcNow, null, false, UserDto.Empty)
		];

		_repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IReadOnlyList<WidgetDto>>.Ok(widgets));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().HaveCount(2);
		result.Should().Contain(d => d.Name == "Bug");
		result.Should().Contain(d => d.Name == "Feature");
	}

	[Fact]
	public async Task HandleList_NoEntities_ReturnsEmptyList()
	{
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<WidgetDto>>.Ok((IReadOnlyList<WidgetDto>)new List<WidgetDto>()));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleList_RepositoryFails_ReturnsEmptyList()
	{
		_repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IReadOnlyList<WidgetDto>>.Fail("Database error"));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleList_RepositoryReturnsNull_ReturnsEmptyList()
	{
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<WidgetDto>>.Ok((IReadOnlyList<WidgetDto>)null!));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().BeEmpty();
	}
}
