// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ProfilePage.razor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Web.Components.Features.Issues;

namespace Web.Components.Features.Profile;

public partial class ProfilePage : ComponentBase
{

	[Inject] private IIssueApiClient IssueClient { get; set; } = null!;

	[Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

	private List<IssueDto> _approved = [];

	private List<IssueDto> _pending = [];

	private List<IssueDto> _rejected = [];

	private bool _isLoading = true;

	private string _userName = "";

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthStateProvider.GetAuthenticationStateAsync();
		_userName = authState.User.Identity?.Name ?? "User";

		_isLoading = true;

		try
		{
			var response = await IssueClient.GetAllAsync(pageSize: 200, authorName: _userName);
			var all = response.Items.ToList();

			_approved = all.Where(i => i is { ApprovedForRelease: true, Rejected: false, Archived: false }).ToList();
			_pending = all.Where(i => i is { ApprovedForRelease: false, Rejected: false, Archived: false }).ToList();
			_rejected = all.Where(i => i.Rejected).ToList();
		}
		finally
		{
			_isLoading = false;
		}
	}

}
