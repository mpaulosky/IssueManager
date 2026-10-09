# TASK

Merge the following branches into the current branch. Each is listed with the commit the host checked:

{{BRANCHES}}

For each branch:

1. Run `git merge <commit> --no-edit` with the commit listed, not the branch name
2. If there are merge conflicts, resolve them intelligently by reading both sides and choosing the correct resolution
3. After merging, run `.sandcastle/check.sh` (build with warnings as errors, and every test project that doesn't
   need Docker). The sandbox has no Docker, on purpose: the host's pre-push gate and CI run the Docker-backed test
   projects
4. If tests fail, fix the issues before proceeding to the next branch

After all branches are merged, make a single commit summarizing the merge.

# CLOSE ISSUES

For each branch that was merged, close its issue using the following command:

`gh issue close <ID> --comment "Completed by Sandcastle"`

Here are all the issues:

{{ISSUES}}

Once you've merged everything you can, output <promise>COMPLETE</promise>.
