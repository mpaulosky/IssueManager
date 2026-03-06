// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     SampleDataPage.razor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Web.Components.Features.Categories;
using Web.Components.Features.Statuses;

namespace Web.Components.Features.Admin;

public partial class SampleDataPage : ComponentBase
{

	[Inject] private ICategoryApiClient CategoryClient { get; set; } = null!;

	[Inject] private IStatusApiClient StatusClient { get; set; } = null!;

	private bool _categoriesCreated;

	private bool _statusesCreated;

	private bool _isWorking;

	private string _errorMessage = "";

	protected override async Task OnInitializedAsync()
	{
		var categories = await CategoryClient.GetAllAsync();
		_categoriesCreated = categories.Any();

		var statuses = await StatusClient.GetAllAsync();
		_statusesCreated = statuses.Any();
	}

	private async Task CreateCategories()
	{
		_isWorking = true;
		_errorMessage = "";

		try
		{
			var existing = await CategoryClient.GetAllAsync();

			if (existing.Any())
			{
				_categoriesCreated = true;

				return;
			}

			string[][] data =
			[
					["Design", "An issue with the design."],
					["Documentation", "An issue with the documentation."],
					["Implementation", "An issue with the implementation."],
					["Clarification", "A quick issue with a general question."],
					["Miscellaneous", "Not sure where this fits."]
			];

			foreach (var d in data)
			{
				await CategoryClient.CreateAsync(new CreateCategoryCommand { CategoryName = d[0], CategoryDescription = d[1] });
			}

			_categoriesCreated = true;
		}
		catch (Exception ex)
		{
			_errorMessage = $"Error creating categories: {ex.Message}";
		}
		finally
		{
			_isWorking = false;
		}
	}

	private async Task CreateStatuses()
	{
		_isWorking = true;
		_errorMessage = "";

		try
		{
			var existing = await StatusClient.GetAllAsync();

			if (existing.Any())
			{
				_statusesCreated = true;

				return;
			}

			(string name, string desc)[] data =
			[
					("Answered", "The suggestion was accepted and the corresponding item was created."),
					("Watching", "The suggestion is interesting. We are watching to see how much interest there is in it."),
					("Upcoming", "The suggestion was accepted and it will be released soon."),
					("Dismissed", "The suggestion was not something that we are going to undertake.")
			];

			foreach (var (name, desc) in data)
			{
				await StatusClient.CreateAsync(new CreateStatusCommand { StatusName = name, StatusDescription = desc });
			}

			_statusesCreated = true;
		}
		catch (Exception ex)
		{
			_errorMessage = $"Error creating statuses: {ex.Message}";
		}
		finally
		{
			_isWorking = false;
		}
	}

}
