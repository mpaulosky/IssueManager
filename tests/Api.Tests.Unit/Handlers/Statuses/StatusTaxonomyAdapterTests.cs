// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusTaxonomyAdapterTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Statuses;

/// <summary>
/// Smoke tests confirming <see cref="StatusTaxonomyAdapter"/> wires StatusDto's fields
/// correctly into <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/>. Behavior shared
/// by every Taxonomy entity (validation rules, not-found handling, etc.) is covered once by
/// TaxonomyCrudHandlerTests, not repeated here.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatusTaxonomyAdapterTests
{
	private readonly IStatusRepository _repository;
	private readonly TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand> _handler;

	public StatusTaxonomyAdapterTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_handler = new TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand>(_repository, StatusTaxonomyAdapter.Instance);
	}

	[Fact]
	public async Task HandleCreate_ValidCommand_MapsToStatusNameAndDescription()
	{
		var command = new CreateStatusCommand { StatusName = "Open", StatusDescription = "Newly filed" };

		_repository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<StatusDto>.Ok(callInfo.Arg<StatusDto>()));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("Open");
		result.Value!.StatusDescription.Should().Be("Newly filed");
	}

	[Fact]
	public async Task HandleUpdate_ValidCommand_MapsToStatusNameAndDescription()
	{
		var statusId = ObjectId.GenerateNewId();
		var existing = new StatusDto(statusId, "Old Name", "Old Description", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateStatusCommand { Id = statusId, StatusName = "Updated Name", StatusDescription = "Updated Description" };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>()).Returns(Result<StatusDto>.Ok(existing));
		_repository.UpdateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<StatusDto>.Ok(callInfo.Arg<StatusDto>()));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("Updated Name");
		result.Value!.StatusDescription.Should().Be("Updated Description");
	}

	[Fact]
	public async Task HandleList_ReturnsStatusesFromRepository()
	{
		IReadOnlyList<StatusDto> statuses = new List<StatusDto>
		{
			new(ObjectId.GenerateNewId(), "Open", "Newly filed", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IReadOnlyList<StatusDto>>.Ok(statuses));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().ContainSingle(s => s.StatusName == "Open");
	}
}
