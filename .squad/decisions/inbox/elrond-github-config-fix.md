# Decision: GitHub Configuration Fix — CODEOWNERS, Templates, Branch Protection

**By:** Elrond (GitHub Ops specialist)  
**Date:** 2026-02-20  
**Status:** Implemented

---

## Problem

IssueManager repository had critical GitHub configuration gaps blocking clean operations:

1. No `.github/CODEOWNERS` → No automatic code owner assignment to PRs
2. No issue/PR templates → No guidance for contributors on expectations
3. No branch protection documentation → No clear enforcement strategy
4. No branching strategy documentation → No clear developer workflow guide

These gaps create friction for the squad:
- PRs don't automatically route to domain experts
- Inconsistent issue/PR quality and format
- Unclear what branch protection rules should be
- Developers unclear on branching/merging best practices

## Decision

Create and commit the following configuration files to `.github/`:

1. **CODEOWNERS** — Squad domain routing for automatic reviewer assignment
2. **ISSUE_TEMPLATE/bug.md** — Bug report template with squad label guidance
3. **ISSUE_TEMPLATE/feature.md** — Feature request template with squad label guidance
4. **PULL_REQUEST_TEMPLATE.md** — PR submission checklist with squad label routing
5. **BRANCH_PROTECTION.md** — Documentation of recommended protection rules and rationale
6. **BRANCHING_STRATEGY.md** — Developer guide for branching, commits, and workflow

## Implementation Details

### CODEOWNERS Routing

Routes files by domain to squad members:

```
/src/**/Domain/                    → Backend (Aragorn)
/src/**/Application/               → Backend (Aragorn)
/src/**/Web/                       → Frontend (Arwen)
/src/**/*.razor*                   → Frontend (Arwen)
/tests/                            → Test (Gimli)
/src/**/AppHost/                   → Infrastructure (Legolas)
/.github/                          → GitHub Ops (Elrond)
/*                                 → Lead (Gandalf) — fallback
```

**Benefit:** PR authors see suggested reviewers; GitHub automatically suggests code owners. Squad members can review within their expertise.

### Issue/PR Templates

All templates include:
- Clear structure (problem → solution → criteria)
- Squad label guidance (`squad:backend`, `squad:frontend`, etc.)
- Checklists (search for duplicates, include tests, etc.)

**Bug Template:**
- Steps to reproduce, expected vs. actual, environment, logs
- Squad domain selector (which agent owns this bug?)

**Feature Template:**
- Problem statement, proposed solution, acceptance criteria
- Squad domain selector

**PR Template:**
- Type of change (bug fix, feature, breaking change, docs)
- Testing checklist
- Squad label requirement
- Review notes section

**Benefit:** Consistent contributor experience; automated triage workflow can parse squad labels reliably.

### Branch Protection Documentation

**BRANCH_PROTECTION.md:**

Recommends protection rules for `main`, `preview`, `dev`:

- **main:** Require 1 code owner review, status checks (build, code quality, main guard), dismiss stale reviews, require conversations resolved
- **preview:** Require 1 reviewer, status checks
- **dev:** Light or no protection, encourage reviews

**Rationale:** Protects production (`main`) with high bar; lighter touch on staging (`preview`) and development (`dev`).

**Enforcement Model:** Platform rules + workflow rules (squad-main-guard.yml already prevents `.ai-team/` from main)

### Branching Strategy Documentation

**BRANCHING_STRATEGY.md:**

Comprehensive developer guide:

1. **Branch naming:** `squad/ISSUE-NUMBER-slug` (ties branches to issues, groups by squad)
2. **Commit convention:** `feat(domain): description | Closes #issue`
3. **Workflow:**
   - Feature branch from `dev` → commit & push → create PR (with `squad:*` label)
   - Automated triage routes to code owner
   - Code owner reviews, approves, merges with "squash and merge"
   - Merge to `preview` for staging
   - Merge to `main` for production
   - Hotfix: branch from `main`, merge back to both `main` and `dev`

4. **Status checks:** Build (dotnet build, tests), Code Quality (CodeQL), Squad Guard (main only)

5. **Best practices:** Small focused PRs, clear messages, request reviews early, don't rebase after review starts

**Benefit:** Clear mental model for developers; reduces confusion about branch flow and merge strategy.

---

## Impact

### Immediate (Next PR)

- New PRs will have `.github/PULL_REQUEST_TEMPLATE.md` auto-populated
- Contributors see squad label guidance in PR form
- Code owners see CODEOWNERS-based suggestions

### Short Term (Next Sprint)

- Bug/feature issues will use new templates (higher consistency)
- Branch protection docs guide repo admin on platform-level settings
- Developers follow branching strategy (fewer conflicts, cleaner history)

### Long Term

- Squad triage workflow becomes more reliable (consistent squad labels)
- PR routing automation works better (CODEOWNERS + labels)
- New contributors have clear onboarding (templates + strategy docs)
- Repository configuration documented (future Elrond maintainers know the setup)

---

## Related Decisions

- **2026-02-18:** GitHub Repository and Process Audit → Identified gaps (this decision addresses them)
- **2026-02-19:** Comprehensive GitHub Configuration Audit → Confirmed templates/CODEOWNERS/protection docs needed

---

## Follow-up

**Still TODO (not part of this decision):**

1. Apply platform-level branch protection rules to `main`, `preview`, `dev` (manual GitHub UI configuration)
2. Fix `.github/README.md` and `SECURITY.md` content mismatch
3. Create `.gitignore` at repository root
4. Consider Conventional Commits enforcement (pre-commit hooks or commit message linting)

**Decision ownership:** Elrond for configuration/documentation; Gandalf for policy decisions requiring lead approval.
