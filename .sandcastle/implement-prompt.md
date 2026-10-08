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

Before each commit, run `.sandcastle/check.sh`. It lints the changed Markdown, builds the solution with warnings as
errors, runs every test project that doesn't need Docker, then the Sandcastle tests. The host runs it too before it
counts the issue complete, so its exit code decides, not what you report.

The sandbox has no Docker, on purpose, so don't try to run `scripts/gate.sh` or the Docker-backed test projects (those
that reference Testcontainers or Aspire.Hosting.Testing). The host's pre-push gate runs them before anything is
pushed, and CI runs every test project.

Only commit work that passes `.sandcastle/check.sh`.

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

Once the task is complete and `.sandcastle/check.sh` passes, output <promise>COMPLETE</promise>. Don't output it for
partial work: the branch is then left unmerged and picked up again in a later round.

# FINAL RULES

ONLY WORK ON A SINGLE TASK.
