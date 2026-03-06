// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoriesPage.razor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

namespace Web.Components.Features.Categories;

/// <summary>Edit model for Radzen DataGrid inline editing.</summary>
public class CategoryEditModel
{

	public string Id { get; set; } = "";

	public string CategoryName { get; set; } = "";

	public string? CategoryDescription { get; set; }

}

public partial class CategoriesPage : ComponentBase
{

	[Inject] private ICategoryApiClient CategoryClient { get; set; } = null!;

	private List<CategoryEditModel> _categories = [];

	private RadzenDataGrid<CategoryEditModel>? _grid;

	private CategoryEditModel? _insertingCategory;

	private CategoryEditModel? _editingCategory;

	private bool _isLoading = true;

	protected override async Task OnInitializedAsync()
	{
		await LoadCategories();
	}

	private async Task LoadCategories()
	{
		_isLoading = true;

		try
		{
			var dtos = await CategoryClient.GetAllAsync();

			_categories = dtos.Select(d => new CategoryEditModel
			{
				Id = d.Id.ToString(),
				CategoryName = d.CategoryName,
				CategoryDescription = d.CategoryDescription
			}).ToList();
		}
		finally
		{
			_isLoading = false;
		}
	}

	private async Task InsertRow()
	{
		_insertingCategory = new CategoryEditModel();
		await _grid!.InsertRow(_insertingCategory);
	}

	private async Task EditRow(CategoryEditModel cat)
	{
		_editingCategory = cat;
		await _grid!.EditRow(cat);
	}

	private async Task SaveRow(CategoryEditModel cat)
	{
		await _grid!.UpdateRow(cat);
	}

	private void CancelEdit(CategoryEditModel cat)
	{
		if (cat == _insertingCategory) _insertingCategory = null;
		if (cat == _editingCategory) _editingCategory = null;
		_grid!.CancelEditRow(cat);
	}

	private async Task OnCreateRow(CategoryEditModel cat)
	{
		_insertingCategory = null;

		var command = new CreateCategoryCommand
		{
			CategoryName = cat.CategoryName,
			CategoryDescription = cat.CategoryDescription
		};

		var created = await CategoryClient.CreateAsync(command);

		if (created != null)
		{
			cat.Id = created.Id.ToString();
		}

		await _grid!.Reload();
	}

	private async Task OnUpdateRow(CategoryEditModel cat)
	{
		_editingCategory = null;

		if (string.IsNullOrEmpty(cat.Id)) return;

		var command = new UpdateCategoryCommand
		{
			CategoryName = cat.CategoryName,
			CategoryDescription = cat.CategoryDescription
		};

		await CategoryClient.UpdateAsync(cat.Id, command);
	}

}
