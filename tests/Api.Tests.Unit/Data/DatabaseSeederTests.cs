// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DatabaseSeederTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Data;

/// <summary>
/// Unit tests for DatabaseSeeder.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseSeederTests
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IStatusRepository _statusRepository;
	private readonly ILogger<DatabaseSeeder> _logger;
	private readonly DatabaseSeeder _seeder;

	public DatabaseSeederTests()
	{
		_categoryRepository = Substitute.For<ICategoryRepository>();
		_statusRepository = Substitute.For<IStatusRepository>();
		_logger = Substitute.For<ILogger<DatabaseSeeder>>();
		_seeder = new DatabaseSeeder(_categoryRepository, _statusRepository, _logger);
	}

	[Fact]
	public async Task SeedAsync_WhenCategoriesExist_DoesNotSeedCategories()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		await _categoryRepository.DidNotReceive()
			.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SeedAsync_WhenCategoriesEmpty_SeedsDefaultCategories()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_categoryRepository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var dto = callInfo.Arg<CategoryDto>();
				return Result<CategoryDto>.Ok(dto with { Id = ObjectId.GenerateNewId() });
			});

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		await _categoryRepository.Received(5)
			.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SeedAsync_WhenStatusesExist_DoesNotSeedStatuses()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		await _statusRepository.DidNotReceive()
			.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SeedAsync_WhenStatusesEmpty_SeedsDefaultStatuses()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var dto = callInfo.Arg<StatusDto>();
				return Result<StatusDto>.Ok(dto with { Id = ObjectId.GenerateNewId() });
			});

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		await _statusRepository.Received(5)
			.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SeedAsync_WhenCategoryCreateFails_LogsWarning()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_categoryRepository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Database error"));

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		_logger.Received().Log(
			LogLevel.Warning,
			Arg.Any<EventId>(),
			Arg.Is<object>(o => o.ToString()!.Contains("Failed to seed category")),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[Fact]
	public async Task SeedAsync_WhenStatusCreateFails_LogsWarning()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Database error"));

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		_logger.Received().Log(
			LogLevel.Warning,
			Arg.Any<EventId>(),
			Arg.Is<object>(o => o.ToString()!.Contains("Failed to seed status")),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[Fact]
	public async Task SeedAsync_WhenCountFails_DoesNotSeedCategories()
	{
		// Arrange
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Fail<long>("Count failed"));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		await _categoryRepository.DidNotReceive()
			.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task SeedAsync_SeedsExpectedCategoryNames()
	{
		// Arrange
		var createdCategories = new List<string>();
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_categoryRepository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var dto = callInfo.Arg<CategoryDto>();
				createdCategories.Add(dto.CategoryName);
				return Result<CategoryDto>.Ok(dto with { Id = ObjectId.GenerateNewId() });
			});

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		createdCategories.Should().BeEquivalentTo(["Bug", "Feature", "Enhancement", "Documentation", "Question"]);
	}

	[Fact]
	public async Task SeedAsync_SeedsExpectedStatusNames()
	{
		// Arrange
		var createdStatuses = new List<string>();
		_categoryRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(5L));
		_statusRepository.CountAsync(Arg.Any<CancellationToken>())
			.Returns(Result.Ok(0L));
		_statusRepository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo =>
			{
				var dto = callInfo.Arg<StatusDto>();
				createdStatuses.Add(dto.StatusName);
				return Result<StatusDto>.Ok(dto with { Id = ObjectId.GenerateNewId() });
			});

		// Act
		await _seeder.SeedAsync(CancellationToken.None);

		// Assert
		createdStatuses.Should().BeEquivalentTo(["Open", "In Progress", "Resolved", "Closed", "Won't Fix"]);
	}
}
