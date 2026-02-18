# Branching Strategy

## Overview

IssueManager uses a **modified Git Flow** strategy with squad-based ownership and GitHub Actions automation. This guide explains how to create, review, and merge branches.

---

## Branch Structure

```
main (production-ready, protected)
  ↑
  └── preview (staging, protected)
        ↑
        └── dev (integration, semi-protected)
              ↑
              └── squad/* (feature branches, temporary)
```

### Branch Roles

| Branch | Purpose | Protection | Merge From | Merge To |
|--------|---------|-----------|-----------|----------|
| `main` | Production | ✅ Full protection | `preview` only | N/A |
| `preview` | Staging/release validation | ✅ Light protection | `dev` only | `main` |
| `dev` | Squad integration | ⚠️ Recommended | `squad/*` branches | `preview` |
| `squad/*` | Feature work | ❌ Unprotected | Any | `dev` |

---

## Workflow: Feature Development

### 1. Create a Feature Branch

Create a branch from `dev` using the naming convention:

```bash
git checkout dev
git pull origin dev
git checkout -b squad/ISSUE-NUMBER-slug
```

**Naming Pattern:** `squad/{issue-number}-{slug}`

**Examples:**
- `squad/42-user-authentication-flow`
- `squad/99-fix-mongodb-connection-pooling`
- `squad/156-blazor-form-validation`

**Guidelines:**
- Use lowercase
- Use hyphens for word separation
- Keep slug under 40 characters
- Reference the GitHub issue number

### 2. Develop & Commit

Make changes on your feature branch. Follow commit message conventions:

```bash
git commit -m "feat(domain): description

- Detailed explanation of change
- Reference issue: Closes #ISSUE_NUMBER"
```

**Conventional Commits Format:**

```
{type}({domain}): {description}

{optional body}

{optional footer: Closes #ISSUE_NUMBER}
```

**Type:** `feat` (feature), `fix` (bug fix), `refactor`, `test`, `docs`, `chore`, `ci`

**Domain:** `backend`, `frontend`, `infra`, `test`, `arch`, `design`

**Examples:**

```bash
git commit -m "feat(backend): add CQRS handler for user creation

- Implements vertical slice for CreateUser command
- Validates email uniqueness
- Returns 201 Created on success

Closes #42"

git commit -m "fix(frontend): prevent form double-submission

Closes #89"
```

### 3. Push & Create Pull Request

Push your branch and create a pull request:

```bash
git push origin squad/ISSUE-NUMBER-slug
```

**On GitHub:**

1. Go to the repository and create a **Pull Request** from your branch into `dev`
2. Title: Follow PR naming: `[SQUAD_LABEL] Short description`
   - Example: `[squad:backend] Add user authentication handler`
3. Description: Use the `.github/PULL_REQUEST_TEMPLATE.md` template
4. **Labels:** Add the appropriate `squad:*` label (auto-routed to code owner)
   - `squad:backend` → Aragorn
   - `squad:frontend` → Arwen
   - `squad:test` → Gimli
   - `squad:infra` → Legolas
   - `squad:arch` → Gandalf

### 4. Automated Triage

The `squad-triage.yml` workflow automatically:

1. Validates your squad label
2. Routes the PR to the appropriate code owner
3. Ensures mutual exclusivity of labels
4. Runs status checks (build, tests, code quality)

**If Triage Fails:**
- Check the workflow logs
- Fix issues (resolve conflicts, failing tests, etc.)
- Push new commits — triage runs again automatically

### 5. Code Review

**For Reviewers (Code Owners):**

1. **Review** code against project standards (see `docs/CONTRIBUTING.md`)
2. **Test** locally if needed
3. **Comment** on specific lines with feedback
4. **Approve** or **Request Changes**

**For PR Authors:**

1. **Address** all review comments
2. **Respond** to feedback (explain decisions, ask clarifications)
3. **Push** new commits (don't force-push or rebase after review starts)
4. **Request re-review** once feedback is addressed

### 6. Merge to `dev`

Once approved and status checks pass:

1. **Use "Squash and merge"** (default)
   - Combines all commits into one
   - Keeps history clean
2. **Commit message:** Use the PR description as commit message
3. **Delete branch** after merge

**On GitHub:** Click "Squash and merge" button, confirm.

---

## Workflow: Release to Staging

When features on `dev` are ready for staging validation:

1. **Create PR** from `dev` into `preview`
2. **Title:** `[RELEASE] Version X.Y.Z - staging release`
3. **Description:** List features/fixes included
4. **Merge strategy:** "Create a merge commit" (preserves branch history for releases)
5. **After merge:** Trigger deployment to staging environment

---

## Workflow: Release to Production

When `preview` (staging) is validated and ready for production:

1. **Create PR** from `preview` into `main`
2. **Title:** `[RELEASE] Version X.Y.Z - production release`
3. **Description:** Release notes, migration notes, breaking changes
4. **Required approvals:** At least 1 code owner + Gandalf (lead)
5. **Merge:** "Squash and merge" is acceptable for simple releases
6. **After merge:** Tag release on `main` (e.g., `v1.2.3`)
7. **Trigger deployment** to production

---

## Workflow: Hotfix (Critical Bug)

For urgent production bugs:

1. **Create branch from `main`:** `hotfix/ISSUE-NUMBER-slug`
2. **Fix the bug** with minimal changes
3. **Create PR into `main`** with urgent details
4. **Require immediate review** from Gandalf + relevant code owner
5. **Merge to `main`** once approved
6. **Cherry-pick or merge back to `dev`** to prevent regression
7. **Tag** as patch release (e.g., `v1.2.1`)

---

## Status Checks

When you push to a feature branch, automated checks run:

### Build Check
```
✅ Build successful
   - dotnet build
   - xUnit tests
   - Code coverage
```

### Code Quality
```
✅ CodeQL scan
   - Security vulnerabilities
   - Code anti-patterns
```

### Squad Main Guard (if merging to main)
```
✅ Squad protection validation
   - Prevents .ai-team/ from main
   - Validates merge safety
```

**If Checks Fail:**
- View the failing check
- Fix issues in your branch
- Push fix — checks re-run automatically

---

## Handling Conflicts

If your PR has merge conflicts:

1. **Pull latest `dev`:**
   ```bash
   git fetch origin
   git rebase origin/dev
   ```

2. **Resolve conflicts** in your editor
3. **Mark as resolved:**
   ```bash
   git add .
   git rebase --continue
   ```

4. **Force-push** to PR (safe after review starts)
   ```bash
   git push origin squad/ISSUE-NUMBER-slug --force-with-lease
   ```

5. **Request re-review**

---

## Best Practices

✅ **Do:**
- Create small, focused PRs (one feature per branch)
- Write clear commit messages
- Add tests for new functionality
- Request reviews early (don't wait until PR is "done")
- Communicate blockers to your code owner

❌ **Don't:**
- Commit directly to `main`, `preview`, or `dev` (except admins for emergencies)
- Merge without approvals
- Ignore failing status checks
- Create long-lived branches (merge within 3-5 days)
- Rebase after code owner reviews (push new commits instead)

---

## Quick Reference

**Create feature branch:**
```bash
git checkout -b squad/ISSUE-NUMBER-slug
```

**Push and create PR:**
```bash
git push origin squad/ISSUE-NUMBER-slug
```

**Update with latest dev:**
```bash
git fetch origin
git rebase origin/dev
```

**Merge to dev (via GitHub UI):**
- Use "Squash and merge"

**Delete branch:**
```bash
git branch -d squad/ISSUE-NUMBER-slug
```

---

## Related Documents

- `.github/BRANCH_PROTECTION.md` - Protection rules and policies
- `.github/PULL_REQUEST_TEMPLATE.md` - PR template (auto-used on GitHub)
- `.github/CODEOWNERS` - Code owner assignment
- `docs/CONTRIBUTING.md` - Full contribution guide
