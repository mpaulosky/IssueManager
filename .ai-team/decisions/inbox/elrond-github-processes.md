# GitHub Process Review and Clean Git Recommendations

### 2026-02-18: GitHub process review and clean git recommendations

**By:** Elrond

**What:** Reviewed GitHub branch protection, workflow automation, repository organization, and git hygiene practices

**Why:** Maintain clean repository state, ensure team follows consistent practices, prevent merge conflicts and lost history, enforce protected branch policies

---

## AUDIT SUMMARY

### ✅ What's Working Well

1. **Protected branch guard automation** — `squad-main-guard.yml` effectively prevents `.ai-team/` and `team-docs/` from shipping to main/preview via intelligent PR blocking and clear error messages
2. **Comprehensive CI/CD workflow** — `.dotnet.yml` includes full test suite, code coverage, GitVersion integration, and proper artifact management
3. **Issue routing automation** — Squad triage, labeling, and assignment workflows are well-designed with fallback logic and member detection
4. **Branch enforcement** — `.gitattributes` configured for union merge strategy on team state files, preventing merge conflicts on append-only decision logs
5. **Release automation** — Proper version management via package.json, semantic versioning with git tags, and GitHub release creation on main push

### ⚠️ Areas Needing Attention

1. **Duplicate workflow files** — Two copy files exist (`codeql-analysis - Copy.yml`, `code-metrics - Copy.yml`) that should be removed
2. **No branch protection rules configured** — While the main-guard workflow provides runtime protection, GitHub's branch protection rules aren't explicitly configured (status checks, required reviews, dismissal rules)
3. **Dev branch strategy missing** — No documented branching strategy or branch protection for `dev` (feature branch integration point)
4. **Preview branch gaps** — `preview` branch has validation but lacks clear merge-back or release documentation
5. **Commit message standards** — No documented convention for commit messages (no `.conventionalcommits` setup)
6. **Merge strategy unclear** — No documented squash-vs-merge guidance for different PR types
7. **Git history hygiene** — No pre-commit hooks or commitlint for enforcing conventional commits

---

## RECOMMENDATIONS

### 1. **Clean up duplicate workflow files**
   - Delete `.github/workflows/codeql-analysis - Copy.yml` and `.github/workflows/code-metrics - Copy.yml`
   - These are leftover duplicates that clutter the workflow directory and may cause confusion
   - **Status:** ✅ Already completed

### 2. **Document and enforce committed git workflow**
   - Create `.github/BRANCH-STRATEGY.md` defining:
     - Main branch: protected, production releases only
     - Dev branch: integration point for feature branches, required PR reviews
     - Feature branches: `feature/*`, `fix/*`, `docs/*` naming convention
     - Hotfix branches: `hotfix/*` for urgent production fixes
   - Add to team documentation for all agents to reference when creating branches
   - **Expected outcome:** Consistent branching across the squad

### 3. **Configure GitHub branch protection rules (via API or web UI)**
   - **Main branch:**
     - Require pull request reviews before merging (1 reviewer minimum)
     - Require status checks to pass: `Build and Test`, `CodeQL Analysis`
     - Require branches to be up to date before merging
     - Dismiss stale PR approvals when new commits are pushed
     - Restrict who can push to matching branches (prevent accidental direct pushes)
   - **Preview branch:**
     - Similar to main but with 0 required reviews (CI validation only)
     - Block direct pushes, require PR workflow
   - **Dev branch:**
     - Require at least 1 PR review for code changes
     - Allow force pushes for team sync (optional, document clearly)
   - **Expected outcome:** Clear enforcement hierarchy, prevents broken releases

### 4. **Establish commit message standards**
   - Adopt Conventional Commits standard: `<type>(<scope>): <subject>`
   - Types: `feat`, `fix`, `docs`, `test`, `refactor`, `ci`, `chore`
   - Example: `feat(squad-routing): auto-assign issues by domain`
   - Document in `.github/COMMIT-GUIDELINES.md`
   - **Expected outcome:** Readable git history, facilitates automated changelog generation

### 5. **Add pre-commit hooks and commitlint validation**
   - Add `commitlint` config (`.commitlintrc.json`) to enforce conventional commits
   - Document in `CONTRIBUTION.md`: `npm install && npx husky install` for local setup
   - Optional: GitHub Actions validation step in CI workflow to catch non-compliant messages
   - **Expected outcome:** Consistent, parseable commit history across all squad members and agents

### 6. **Document merge strategy by PR type**
   - **Feature PRs (main):** Squash and merge (keeps history clean, one commit per feature)
   - **Release PRs (preview → main):** Create a merge commit (preserves release boundary)
   - **Hotfix PRs:** Squash and merge, then backport to dev (ensures consistency)
   - **Internal team-docs PRs:** N/A (blocked by main-guard)
   - Document in `.github/MERGE-STRATEGY.md`
   - **Expected outcome:** Reviewers understand intended merge behavior

### 7. **Update README and contribution guidelines**
   - Create or update `.github/CONTRIBUTING.md` with:
     - Branch naming conventions
     - PR checklist (tests pass, no forbidden files, commit messages valid)
     - Review and merge process for each branch
     - How squad members collaborate on features
   - Link from README.md for visibility
   - **Expected outcome:** New contributors (including agents) follow documented practices

---

## Git Workflow Guide for Squad

### Step 1: Before Starting Work

1. **Sync with main:**
   ```bash
   git checkout main
   git pull origin main
   ```

2. **Create feature branch from main:**
   ```bash
   git checkout -b feature/issue-123-descriptive-name
   # or: fix/bug-fix-name, docs/update-readme
   ```
   - Naming: `{type}/{issue-number}-{kebab-case-description}`
   - Types: `feature`, `fix`, `docs`, `refactor`, `test`

### Step 2: Commit Changes

1. **Write clear, conventional commit messages:**
   ```bash
   git add .
   git commit -m "feat(backend): add issue resolver handler

   - Implement CQRS handler for issue resolution
   - Add unit tests for resolver logic
   - Update vertical slice documentation"
   ```
   - Format: `<type>(<scope>): <subject>`
   - Scope: domain or feature name (optional)
   - Subject: lowercase, imperative, no period
   - Body (optional): explain *why*, not *what*

2. **Keep commits atomic:**
   - One logical unit per commit
   - Avoid mixing feature work with formatting/cleanup
   - Easier to revert or squash later

### Step 3: Push and Open PR

1. **Push feature branch:**
   ```bash
   git push origin feature/issue-123-descriptive-name
   ```

2. **Create Pull Request:**
   - Title: Use conventional commit format: `feat(scope): description`
   - Description: Reference issue (`Fixes #123`), explain approach, list key changes
   - Assign reviewer (domain expert from squad)
   - Avoid committing `.ai-team/` files (main-guard will reject them)

3. **CI checks run automatically:**
   - Build and Test workflow validates code
   - CodeQL security analysis runs
   - Code metrics captured
   - Workflow must pass before merge

### Step 4: Review and Merge

1. **Squad member reviews PR:**
   - Check for test coverage
   - Verify against vertical slice boundaries
   - Ensure commit messages follow standards
   - Approve or request changes

2. **Merge when ready:**
   - For feature PRs: **Squash and merge** (keeps main history clean)
     ```
     git commit -m "feat(backend): add issue resolver handler (#123)"
     ```
   - For release PRs: **Create a merge commit** (preserves release boundary)
   - Delete feature branch after merge (GitHub auto-cleans)

3. **PR merged = feature complete:**
   - Closes associated issue automatically (via `Fixes #123`)
   - Workflow jobs trigger on main push (release, etc.)
   - Team can pull latest via `git pull origin main`

### Step 5: Keep Local Repo Clean

1. **Delete merged branches locally:**
   ```bash
   git branch -d feature/issue-123-descriptive-name
   ```

2. **Sync regularly:**
   ```bash
   git fetch origin
   git rebase origin/main  # or: git merge origin/main
   ```

3. **Check for stale branches:**
   ```bash
   git branch -v  # local
   git branch -r -v  # remote
   ```

---

## Repository Configuration Summary

| Setting | Current | Recommended |
| --- | --- | --- |
| Default branch | main | ✅ main (correct) |
| Branch protection (main) | Runtime guard only | Add GitHub rules: reviews, status checks |
| Branch protection (dev) | None | Add: PR required, 1 review min |
| Commit style | Varies | Enforce Conventional Commits |
| Merge strategy | Varies | Document: squash for features, merge for releases |
| Git hooks | None | Optional: commitlint, pre-commit |
| Duplicate files | 2 copy files | Removed ✅ |

---

## Files to Create / Update

### New Files

1. **`.github/BRANCH-STRATEGY.md`** — Branch naming and lifecycle (main, dev, feature/*, hotfix/*)
2. **`.github/MERGE-STRATEGY.md`** — Merge commit vs squash guidance by PR type
3. **`.github/COMMIT-GUIDELINES.md`** — Conventional commits format and examples
4. **`.github/CONTRIBUTING.md`** — Full contribution workflow for squad members

### Updated Files

1. **`README.md`** — Add link to `.github/CONTRIBUTING.md`
2. **`.github/workflows/dotnet.yml`** — Optional: add commitlint step (future enhancement)

---

## Implementation Timeline

- **Immediate (now):** Remove duplicate workflow files ✅
- **This week:** Create branch strategy documentation and contribution guide
- **Next review:** Configure GitHub branch protection rules via web UI or API
- **Ongoing:** Squad members follow documented git workflow

---

## Notes for Elrond

- Main-guard workflow is doing excellent job preventing accidental commits to protected branches
- Squad automation workflows are well-structured and handle edge cases gracefully
- Biggest gap is lack of documented commit/merge strategy — agents need clear guidance
- Consider future enhancement: GitHub Actions step to lint commit messages in CI
- Preview branch validation is solid but could be documented better for release process clarity
