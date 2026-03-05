// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DatabaseSeeder.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

/// <summary>
/// Seeds default Category and Status data if the collections are empty.
/// </summary>
public class DatabaseSeeder
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IStatusRepository _statusRepository;
	private readonly ILogger<DatabaseSeeder> _logger;

	public DatabaseSeeder(
		ICategoryRepository categoryRepository,
		IStatusRepository statusRepository,
		ILogger<DatabaseSeeder> logger)
	{
		_categoryRepository = categoryRepository;
		_statusRepository = statusRepository;
		_logger = logger;
	}

	/// <summary>
	/// Seeds default data if collections are empty.
	/// </summary>
	public async Task SeedAsync(CancellationToken cancellationToken = default)
	{
		await SeedCategoriesAsync(cancellationToken);
		await SeedStatusesAsync(cancellationToken);
	}

	private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
	{
		var countResult = await _categoryRepository.CountAsync(cancellationToken);
		if (!countResult.Success || countResult.Value > 0)
		{
			_logger.LogInformation("Categories already seeded ({Count} existing). Skipping.",
				countResult.Success ? countResult.Value : 0);
			return;
		}

		_logger.LogInformation("Seeding default categories...");

		var categories = new[]
		{
			new CategoryDto(
				ObjectId.Empty,
				"Bug",
				"A bug report or defect.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new CategoryDto(
				ObjectId.Empty,
				"Feature",
				"A new feature request.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new CategoryDto(
				ObjectId.Empty,
				"Enhancement",
				"An improvement to existing functionality.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new CategoryDto(
				ObjectId.Empty,
				"Documentation",
				"Documentation updates or additions.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new CategoryDto(
				ObjectId.Empty,
				"Question",
				"A question or request for information.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
		};

		foreach (var category in categories)
		{
			var result = await _categoryRepository.CreateAsync(category, cancellationToken);
			if (result.Success)
				_logger.LogInformation("Seeded category: {Name}", category.CategoryName);
			else
				_logger.LogWarning("Failed to seed category {Name}: {Error}", category.CategoryName, result.Error);
		}
	}

	private async Task SeedStatusesAsync(CancellationToken cancellationToken)
	{
		var countResult = await _statusRepository.CountAsync(cancellationToken);
		if (!countResult.Success || countResult.Value > 0)
		{
			_logger.LogInformation("Statuses already seeded ({Count} existing). Skipping.",
				countResult.Success ? countResult.Value : 0);
			return;
		}

		_logger.LogInformation("Seeding default statuses...");

		var statuses = new[]
		{
			new StatusDto(
				ObjectId.Empty,
				"Open",
				"The issue is open and awaiting review.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new StatusDto(
				ObjectId.Empty,
				"In Progress",
				"The issue is actively being worked on.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new StatusDto(
				ObjectId.Empty,
				"Resolved",
				"The issue has been resolved.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new StatusDto(
				ObjectId.Empty,
				"Closed",
				"The issue is closed.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
			new StatusDto(
				ObjectId.Empty,
				"Won't Fix",
				"The issue will not be fixed.",
				DateTime.UtcNow,
				null,
				false,
				UserDto.Empty),
		};

		foreach (var status in statuses)
		{
			var result = await _statusRepository.CreateAsync(status, cancellationToken);
			if (result.Success)
				_logger.LogInformation("Seeded status: {Name}", status.StatusName);
			else
				_logger.LogWarning("Failed to seed status {Name}: {Error}", status.StatusName, result.Error);
		}
	}
}
