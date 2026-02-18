# History — Elrond

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**

- C#, Aspire, Blazor, .NET 10.0
- Vertical Slice Architecture
- MongoDB.EntityFramework + CQRS
- GitHub-based workflow and issue tracking

**Squad Setup:**

- Universe: The Lord of the Rings
- Team members: Gandalf (Lead), Aragorn (Backend), Arwen (Frontend), Gimli (Tester), Legolas (DevOps/Infrastructure), Galadriel (Designer)
- Scribe and Ralph for session management and work monitoring
- Routing: Architecture, Backend, Frontend, Design, QA, Infrastructure domains

**GitHub Integration:**

- Issue labels: `squad:*` for agent routing
- Squad automation workflows in `.github/workflows/`
- Repository setup in [https://github.com/mpaulosky/IssueManager](https://github.com/mpaulosky/IssueManager)

**Workflow automation:**

- Issue triage and labeling
- PR workflow management
- Agent availability and readiness monitoring

---

## Learnings

### 2026-02-18: GitHub Repository and Process Audit

**Repository Organization Observations:**
- Repository uses `main` as default branch with protected guard workflows (`squad-main-guard.yml`)
- Clean separation: team state (`.ai-team/`) prevented from shipping to main via runtime checks
- `.gitattributes` configured for union merge strategy on team state files (smart merge conflict prevention)
- Duplicate workflow files (`*Copy.yml`) cleaned up during audit

**Branch Protection & Workflow Enforcement:**
- Main-guard workflow effectively prevents forbidden paths (`.ai-team/`, `team-docs/`) with clear error messages and recovery instructions
- No explicit GitHub branch protection rules configured (gap: no enforced reviews, status checks at platform level)
- Preview branch has validation but lacks clear merge-back documentation
- Dev branch strategy not formally documented (team integrates features here)

**Git Hygiene & Commit Standards:**
- No documented commit message standard (commits vary in style)
- No enforced merge strategy (squash vs merge behavior not standardized)
- Conventional Commits approach recommended but not yet implemented
- Pre-commit hooks absent (optional enhancement for commitment enforcement)

**GitHub Workflows & Automation:**
- Squad automation workflows are comprehensive and well-designed:
  - Issue triage with keyword routing and fallback logic
  - Label enforcement with mutual exclusivity rules
  - PR guard validation for protected branches
  - Release automation with GitVersion integration
  - CI/CD with full test suite, code coverage, artifact management
- Workflow structure supports squad:* label routing effectively
- Ralph heartbeat monitors board health and auto-triages unassigned issues

**Team Collaboration Patterns:**
- Feature branches flow through main with PR review gates
- Release builds trigger on main push, hotfixes via feature branches
- Agent routing via labels (`squad:*`) enables autonomous issue assignment
- Vertical slice architecture supported via routing rules and domain-based triage

**Recommendations Documented:**
1. Clean up duplicate workflow files ✅
2. Document formal branch strategy (.github/BRANCH-STRATEGY.md)
3. Configure GitHub branch protection rules (reviews, status checks, dismissal)
4. Establish Conventional Commits standard for commit messages
5. Document merge strategy by PR type (squash for features, merge for releases)
6. Create CONTRIBUTING.md with team's git workflow guide

**Key Insight:** The squad's automation is strong; documentation and platform-level branch protection are the main improvements. Once implemented, the team will have a clean, auditable git workflow.

### 2026-02-19: Comprehensive GitHub Configuration Audit

**Repository Documentation Patterns:**
- README.md content mismatch — file contained dotfiles project content instead of IssueManager
- SECURITY.md content mismatch — referenced AINotesApp instead of IssueManager
- CONTRIBUTING.md exists in docs/ with comprehensive content but includes generic placeholders
- CODE_OF_CONDUCT.md exists in docs/ (Contributor Covenant v2.0)
- Best practice: Root or .github/ location for CONTRIBUTING/CODE_OF_CONDUCT for better discoverability

**Missing Critical Files:**
- No .gitignore at root (risk of committing build artifacts, IDE files, sensitive data)
- No .github/CODEOWNERS (no automatic reviewer assignment)
- No .github/ISSUE_TEMPLATE/ directory (no structured issue forms)
- No .github/PULL_REQUEST_TEMPLATE.md (no PR checklist)

**Workflow Audit Findings:**
- 13 workflows total: 10 squad automation workflows + 3 application workflows
- Squad workflows are sophisticated: triage, label enforcement, heartbeat monitoring, assignment
- @copilot integration with capability tiers (good-fit, needs-review, not-suitable)
- Ralph (heartbeat) auto-triages untriaged issues and monitors board health
- Workflow permissions are minimal and secure (best practice)
- squad-ci.yml appears redundant with dotnet.yml (ci only tests Node.js, dotnet.yml tests full .NET)
- All workflows use actions/checkout@v4+, actions/setup-node@v4, actions/setup-dotnet@v5 (modern versions)

**Security & Dependencies:**
- CodeQL configured for C# (weekly + push/PR triggers)
- Dependabot configured for GitHub Actions, NuGet, .NET SDK with weekly updates
- Dependabot directory paths point to `/nuget/helpers/lib/NuGetUpdater` (may be incorrect)
- Codecov.yml configured with informational status (won't block PRs)
- PAT secret COPILOT_ASSIGN_TOKEN used for @copilot agent assignment (graceful fallback to GITHUB_TOKEN)

**Branch Protection & Strategy:**
- No GitHub platform-level branch protection rules configured
- Protection enforced via workflow only (squad-main-guard.yml blocks .ai-team/ from main/preview)
- No formal branch strategy documentation
- Observed flow: feature → dev → preview → main
- .gitattributes uses union merge for team state files (smart conflict prevention)

**Label System Design:**
- Automated label sync from .ai-team/team.md via sync-squad-labels.yml
- Namespace design: squad:*, go:*, release:*, type:*, priority:*
- Mutual exclusivity enforced via squad-label-enforce.yml
- Color palette: SQUAD_COLOR (9B8FCC), COPILOT_COLOR (10b981), high-signal colors (bug=FF0422, feedback=00E5FF)
- High-signal labels visually dominate (intentional design for important issues)

**Workflow Architecture Patterns:**
- Triage → label enforcement → assignment → heartbeat monitoring (autonomous loop)
- Keyword-based routing matches .ai-team/routing.md domains
- @copilot capability evaluation integrated into triage workflow
- Ralph (heartbeat) acts as autonomous work monitor with auto-triage fallback
- squad:* labels auto-created from team roster (self-healing label system)

**GitHub Actions Best Practices Observed:**
- Permissions scoped per-job (contents: read, issues: write, etc.)
- Conditional job execution (if: github.event.label.name == 'squad')
- Pagination for large API responses (pull request file lists)
- Error handling with graceful fallbacks (copilot assignment fallback to standard API)
- Descriptive workflow names and job outputs
- Concurrency groups for resource contention (pages deployment)

**Key Insight:** The repository has enterprise-grade automation but lacks foundational documentation files. The squad workflow system is the strongest element — sophisticated triage, labeling, and autonomous monitoring. Main improvements: fix README, add .gitignore/CODEOWNERS, create issue/PR templates, configure platform branch protection.

### 2026-02-20: GitHub Configuration Fix — Critical Files Created

**Completed Actions:**
- Created `.github/CODEOWNERS` with squad domain routing (backend/Aragorn, frontend/Arwen, test/Gimli, infra/Legolas, design/Galadriel, arch/Gandalf, ops/Elrond)
- Created `.github/ISSUE_TEMPLATE/bug.md` with structured bug report form and squad label guidance
- Created `.github/ISSUE_TEMPLATE/feature.md` with feature request form and acceptance criteria
- Created `.github/PULL_REQUEST_TEMPLATE.md` with PR checklist, squad label routing, and review notes
- Created `.github/BRANCH_PROTECTION.md` documenting recommended protection rules (require reviews, status checks, dismissal of stale reviews, conversation resolution)
- Created `.github/BRANCHING_STRATEGY.md` with full developer workflow guide (branch naming, commit conventions, feature/release/hotfix workflows, status checks, conflict handling)

**CODEOWNERS Pattern:**
- Domain-based file routing: `/src/**/Domain/` → backend, `/src/**/Web/` → frontend, `/tests/` → test, etc.
- Fallback to lead (`@mpaulosky`) for unmatched paths
- Automatic PR assignment to code owners per changed files

**Issue/PR Templates Pattern:**
- Bug template includes environment, reproduction steps, error logs, and domain selector
- Feature template includes problem statement, proposed solution, and acceptance criteria
- PR template includes change type selector, testing checklist, and squad label requirement
- All templates guide users to add `squad:*` labels for automatic triage routing

**Branch Protection Rationale:**
- `main` (full protection): 1 code owner review, status checks, stale review dismissal, conversation resolution
- `preview` (light protection): 1 reviewer, status checks
- `dev` (recommended, not enforced): 1 reviewer for stability
- Emphasizes automated squad-based triage via PR labels
- Documents hotfix bypass procedures (Gandalf + domain expert approval for emergencies)

**Branching Strategy Coverage:**
- `squad/ISSUE-NUMBER-slug` naming convention ties branches to GitHub issues
- Squash-merge strategy for features (clean linear history)
- Conventional Commits format: `{type}({domain}): description`
- Full feature → dev → preview → main workflow documented
- Hotfix workflow from `main` documented
- Conflict resolution and rebase best practices included
- Status check expectations (build, code quality, squad guard) documented

**Key Patterns Established:**
- CODEOWNERS + PULL_REQUEST_TEMPLATE bridge the gap between automation and human review
- Template guidance on squad labels ensures workflow triggers correctly
- Branch protection docs clarify what's enforced vs. recommended vs. optional
- Branching strategy docs give developers clear mental model of the repo flow

**Outstanding Items:**
- GitHub platform-level branch protection rules still need manual configuration in repo settings
- .gitignore at repository root still missing (noted in earlier audit)
- README.md and SECURITY.md content mismatch still needs correction
