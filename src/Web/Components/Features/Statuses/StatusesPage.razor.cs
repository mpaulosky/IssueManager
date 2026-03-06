// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusesPage.razor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

namespace Web.Components.Features.Statuses;

/// <summary>Edit model for Radzen DataGrid inline editing.</summary>
public class StatusEditModel
{
	public string Id { get; set; } = "";
	public string StatusName { get; set; } = "";
	public string? StatusDescription { get; set; }
}

public partial class StatusesPage : ComponentBase
{
	[Inject] private IStatusApiClient StatusClient { get; set; } = null!;

	private List<StatusEditModel> _statuses = [];
	private readonly RadzenDataGrid<StatusEditModel>? _grid;
	private StatusEditModel? _insertingStatus;
	private StatusEditModel? _editingStatus;
	private bool _isLoading = true;

	protected override async Task OnInitializedAsync()
	{
		await LoadStatuses();
	}

	private async Task LoadStatuses()
	{
		_isLoading = true;
		try
		{
			var dtos = await StatusClient.GetAllAsync();
			_statuses = dtos.Select(d => new StatusEditModel
			{
				Id = d.Id.ToString(),
				StatusName = d.StatusName,
				StatusDescription = d.StatusDescription
			}).ToList();
		}
		finally
		{
			_isLoading = false;
		}
	}

	private async Task InsertRow()
	{
		_insertingStatus = new StatusEditModel();
		await _grid!.InsertRow(_insertingStatus);
	}

	private async Task EditRow(StatusEditModel status)
	{
		_editingStatus = status;
		await _grid!.EditRow(status);
	}

	private async Task SaveRow(StatusEditModel status)
	{
		await _grid!.UpdateRow(status);
	}

	private void CancelEdit(StatusEditModel status)
	{
		if (status == _insertingStatus) _insertingStatus = null;
		if (status == _editingStatus) _editingStatus = null;
		_grid!.CancelEditRow(status);
	}

	private async Task OnCreateRow(StatusEditModel status)
	{
		_insertingStatus = null;
		var command = new CreateStatusCommand
		{
			StatusName = status.StatusName,
			StatusDescription = status.StatusDescription
		};
		var created = await StatusClient.CreateAsync(command);
		if (created != null)
		{
			status.Id = created.Id.ToString();
		}
		await _grid!.Reload();
	}

	private async Task OnUpdateRow(StatusEditModel status)
	{
		_editingStatus = null;
		if (string.IsNullOrEmpty(status.Id)) return;
		var command = new UpdateStatusCommand
		{
			StatusName = status.StatusName,
			StatusDescription = status.StatusDescription
		};
		await StatusClient.UpdateAsync(status.Id, command);
	}
}
