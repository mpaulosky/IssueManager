### 2026-02-24T202205Z: Issue-to-sprint assignment directive

**By:** Matthew Paulosky (via Copilot)

**What:** When the team creates a GitHub issue, it should be automatically added to the current sprint or optionally to a new sprint. Issues should not remain unscheduled.

**Why:** User request — process rule for work management and sprint planning. Ensures issues are tracked and scheduled rather than floating in the backlog.

**Applies to:** All agents creating issues via `gh issue create` or manual GitHub creation.

**Implementation note:** When spawning agents to create issues, include: "After creating the issue, add it to the current sprint via `gh issue edit` with the `--add-assignee-projects` flag or by editing the GitHub Projects UI."
