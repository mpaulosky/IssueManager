# Branch Protection Policy

## Overview

This document defines the branch protection strategy for the IssueManager repository. Branch protection rules enforce code quality, security, and team processes at the GitHub platform level.

## Protected Branches

### `main` (Production)

The `main` branch is the production-ready state of IssueManager. All changes must go through a rigorous review and validation process.

**Protection Rules:**

1. **Require pull request reviews before merging**
   - Minimum reviewers: 1
   - Dismiss stale pull request approvals: Yes
   - Require review from code owners: Yes
   - Restrictions: None

2. **Require status checks to pass before merging**
   - Required checks:
     - `build` (dotnet build, xUnit tests, code coverage)
     - `code-quality` (CodeQL security scanning)
     - `squad-main-guard` (validates .ai-team/ protection)
   - Require branches to be up to date before merging: Yes

3. **Require conversation resolution before merging**
   - Dismisses pull request reviews when someone pushes a new commit: Yes

4. **Require signed commits**
   - Recommended: Yes (not enforced, but strongly encouraged)

5. **Enforce admins**
   - Yes (admins cannot bypass protection rules)

**Why These Rules?**

- **Single reviewer requirement** ensures human oversight without slowing delivery
- **Code owner review requirement** routes changes to domain experts (via `.github/CODEOWNERS`)
- **Status checks** ensure code quality, security scanning, and test passing
- **Stale PR dismissal** requires reviewers to re-approve after new commits (security best practice)
- **Conversation resolution** prevents merging PRs with unresolved feedback
- **Admin enforcement** ensures no one, including repository admins, can bypass the process

### `preview` (Staging)

The `preview` branch is the staging environment where features are validated before production.

**Protection Rules:**

1. **Require pull request reviews before merging**
   - Minimum reviewers: 1
   - Dismiss stale pull request approvals: Yes

2. **Require status checks to pass before merging**
   - Required checks: `build`, `code-quality`

3. **Require conversation resolution before merging**
   - Yes

**Why These Rules?**

- Lighter touch than `main` but ensures stability for staging tests
- Allows faster iteration than `main` while maintaining code quality

### `dev` (Development)

The `dev` branch is where squad members integrate feature branches for collaborative development.

**Recommended Rules** (optional, but suggested):

1. **Require pull request reviews before merging**
   - Minimum reviewers: 1 (or none if team prefers rapid integration)

2. **Status checks**
   - Require `build` to pass

**Rationale:** Development branch can move faster than `main`, but should still validate basic build integrity.

---

## Merge Strategies

### Recommended Merge Type: **Squash and Merge**

**When to use:**
- Feature branches into `dev` or `main`
- Keeps commit history clean and linear
- One commit per feature/fix in `main` for easier rollback

**Commit message format:** `feat(domain): description | Closes #issue`

### Alternative: **Create a merge commit**

**When to use:**
- Release merges (to preserve branch history)
- Complex multi-commit changes where history matters

---

## Branch Naming Convention

Feature branches should follow this naming pattern:

```
squad/{issue-number}-{slug}
```

**Examples:**
- `squad/123-user-authentication`
- `squad/456-fix-mongodb-connection`
- `squad/789-blazor-component-refactor`

**Pattern Benefits:**
- Ties branches to GitHub issues
- Groups branches by squad/feature area
- Makes PR filtering and tracing easier

---

## Pull Request Review Workflow

1. **Create PR** from feature branch into target branch (`dev` or `main`)
   - PR must include a squad label (`squad:*`)
   - PR must have a clear description (see `.github/PULL_REQUEST_TEMPLATE.md`)

2. **Automated Triage** (via `squad-triage.yml` workflow)
   - Validates squad labels
   - Routes PR to appropriate code owner

3. **Review & Approval**
   - Minimum 1 code owner review required
   - All conversations must be resolved
   - Stale reviews dismissed on new commits

4. **Status Checks**
   - Build must pass (dotnet build, tests)
   - CodeQL security scan must pass
   - Main guard check must pass (if merging to `main`)

5. **Merge**
   - Use "Squash and merge" (default)
   - Delete feature branch after merge

---

## Bypass & Emergency Procedures

### Emergency Hotfix (Critical Production Bug)

If a critical bug in production requires bypassing normal review:

1. Create a hotfix branch from `main`: `hotfix/issue-number-slug`
2. Request immediate review from at least 2 code owners (Gandalf + domain expert)
3. Once approved, merge directly to `main` (status checks still required)
4. Cherry-pick or merge back to `dev` after `main` merge
5. Log the incident in `.ai-team/decisions/` for post-review

**Admin Override:** Only Gandalf (lead) may override branch protection rules, and only for documented emergencies.

---

## Enforcement & Monitoring

- **Audit:** Review branch protection settings monthly or after policy changes
- **Violations:** Unauthorized direct pushes or bypasses trigger alerts
- **Compliance:** All squad members must follow branching and review policies
- **Documentation:** This policy is reviewed and updated as team practices evolve

---

## Related Documents

- `.github/BRANCHING_STRATEGY.md` - Detailed branching and workflow guide
- `.github/CODEOWNERS` - Automatic code owner assignment
- `docs/CONTRIBUTING.md` - Full contribution guidelines
