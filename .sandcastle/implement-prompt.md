# TASK

Fix issue {{TASK_ID}}: {{ISSUE_TITLE}}

Pull in the issue using `gh issue view <ID>`. If it has a parent PRD, pull that in too.

Only work on the issue specified.

Work on branch {{BRANCH}}. Make commits and run tests.

# CONTEXT

Here are the last 10 commits:

<recent-commits>

!`git log -n 10 --format="%H%n%ad%n%B---" --date=short`

</recent-commits>

# EXPLORATION

Explore the repo and fill your context window with relevant information that will allow you to complete the task.

Pay extra attention to test files that touch the relevant parts of the code.

# EXECUTION

If applicable, use RGR to complete the task.

1. RED: write one test
2. GREEN: write the implementation to pass that test
3. REPEAT until done
4. REFACTOR the code

# FEEDBACK LOOPS

Run the repository's gate, the same one the pre-push hook runs: `bash scripts/gate.sh`. It lints the changed files,
builds the solution with warnings as errors, and runs every test project.

The sandbox has no Docker, so the Docker-backed test projects (`tests/Api.Tests.Integration` and
`tests/AppHost.Tests.E2E`) can't run here. If the gate gets that far and fails only on one of them for want of Docker,
run each remaining test project from `python3 .github/scripts/discover_tests.py --list` with
`dotnet test <project> --configuration Release`. The pre-push hook on the host runs the full gate before anything is
pushed.

Only commit work that passes the gate.

# COMMIT

Make a git commit. The commit message must:

1. Start with `RALPH:` prefix
2. Include task completed + PRD reference
3. Key decisions made
4. Files changed
5. Blockers or notes for next iteration

Keep it concise.

# THE ISSUE

If the task is not complete, leave a comment on the issue with what was done.

Do not close the issue - this will be done later.

Once the task is complete and the gate passes, output <promise>COMPLETE</promise>. Don't output it for partial work:
the branch is then left unmerged and picked up again in a later round.

# FINAL RULES

ONLY WORK ON A SINGLE TASK.
