// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AdminPage.razor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Web.Components.Features.Issues;

namespace Web.Components.Features.Admin;

public partial class AdminPage : ComponentBase
{

	[Inject] private IIssueApiClient IssueClient { get; set; } = null!;

	private List<IssueDto> _issues = [];

	private bool _isLoading = true;

	private string? _editingTitleId;

	private string? _editingDescId;

	private string _editedTitle = "";

	private string _editedDesc = "";

	protected override async Task OnInitializedAsync()
	{
		_isLoading = true;

		try
		{
			var response = await IssueClient.GetAllAsync(pageSize: 100);

			_issues = response.Items
					.Where(i => i is { ApprovedForRelease: false, Rejected: false })
					.ToList();
		}
		finally
		{
			_isLoading = false;
		}
	}

	private async Task ApproveIssue(IssueDto issue)
	{
		var command = new UpdateIssueCommand
		{
			Id = issue.Id,
			Title = issue.Title,
			Description = issue.Description,
			ApprovedForRelease = true,
			Rejected = false
		};

		var updated = await IssueClient.UpdateAsync(issue.Id.ToString(), command);

		if (updated != null)
		{
			_issues.Remove(issue);
		}
	}

	private async Task RejectIssue(IssueDto issue)
	{
		var command = new UpdateIssueCommand
		{
			Id = issue.Id,
			Title = issue.Title,
			Description = issue.Description,
			Rejected = true
		};

		var updated = await IssueClient.UpdateAsync(issue.Id.ToString(), command);

		if (updated != null)
		{
			_issues.Remove(issue);
		}
	}

	private void EditTitle(IssueDto issue)
	{
		_editedTitle = issue.Title;
		_editingTitleId = issue.Id.ToString();
		_editingDescId = null;
	}

	private async Task SaveTitle(IssueDto issue)
	{
		var command = new UpdateIssueCommand { Id = issue.Id, Title = _editedTitle, Description = issue.Description };
		var updated = await IssueClient.UpdateAsync(issue.Id.ToString(), command);

		if (updated != null)
		{
			var idx = _issues.IndexOf(issue);
			if (idx >= 0) _issues[idx] = updated;
		}

		_editingTitleId = null;
	}

	private void EditDescription(IssueDto issue)
	{
		_editedDesc = issue.Description;
		_editingDescId = issue.Id.ToString();
		_editingTitleId = null;
	}

	private async Task SaveDescription(IssueDto issue)
	{
		var command = new UpdateIssueCommand { Id = issue.Id, Title = issue.Title, Description = _editedDesc };
		var updated = await IssueClient.UpdateAsync(issue.Id.ToString(), command);

		if (updated != null)
		{
			var idx = _issues.IndexOf(issue);
			if (idx >= 0) _issues[idx] = updated;
		}

		_editingDescId = null;
	}

}
