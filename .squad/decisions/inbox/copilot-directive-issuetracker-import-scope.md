### 2026-03-05T12:17Z: IssueTrackerApp → IssueManager import — revised scope
**By:** Matthew Paulosky (via Copilot)
**What:**
- DO NOT modify IssueTrackerApp at all — it stays untouched
- IMPORT the Components, Pages, and Shared folders from IssueTrackerApp INTO IssueManager's Web project
- Update the IMPORTED files to match IssueManager styling (Tailwind CSS, CSS custom properties, dark/light theme) and functionality (Auth0, Aspire HTTP clients, IssueManager patterns)
- Auth: replace Azure AD / Microsoft Identity with Auth0 (matching IssueManager's AuthExtensions + /auth/login /auth/logout pattern)
- Data access: replace direct service injection (ICategoryService, IIssueService, etc.) with IssueManager's HTTP API clients
- Keep Radzen DataGrid for admin pages (Categories, Statuses) — style it to match IssueManager CSS vars
- Keep Blazored.SessionStorage filter persistence from IssueTrackerApp Index page
**Why:** User clarified that IssueTrackerApp is the source of content; IssueManager is the target destination and design system.
