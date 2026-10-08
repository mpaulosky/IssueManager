# TASK

Merge the following branches into the current branch:

{{BRANCHES}}

For each branch:

1. Run `git merge <branch> --no-edit`
2. If there are merge conflicts, resolve them intelligently by reading both sides and choosing the correct resolution
3. After merging, run the repository's gate, `bash scripts/gate.sh` (lint, build with warnings as errors, and every
   test project). The sandbox has no Docker, so if the gate fails only on `tests/Api.Tests.Integration` or
   `tests/AppHost.Tests.E2E` for want of Docker, run each remaining project from
   `python3 .github/scripts/discover_tests.py --list` with `dotnet test <project> --configuration Release`
4. If tests fail, fix the issues before proceeding to the next branch

After all branches are merged, make a single commit summarizing the merge.

# CLOSE ISSUES

For each branch that was merged, close its issue using the following command:

`gh issue close <ID> --comment "Completed by Sandcastle"`

Here are all the issues:

{{ISSUES}}

Once you've merged everything you can, output <promise>COMPLETE</promise>.
