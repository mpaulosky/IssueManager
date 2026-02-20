# Skill: GitHub Repository Audit

**Domain:** GitHub Operations, DevOps  
**Complexity:** Medium  
**Reusability:** High

## Description

Systematic audit of a GitHub repository's configuration, workflows, documentation, security, and best practices. Produces a comprehensive report with prioritized recommendations.

## When to Use

- Initial repository setup review
- Pre-production security check
- Quarterly maintenance audit
- Onboarding new team members (assess documentation quality)
- After major workflow changes
- Compliance review

## Audit Checklist

### 1. Documentation Files
- [ ] **README.md** — Project overview, setup, build instructions
- [ ] **CONTRIBUTING.md** — Contribution guide (root or .github/)
- [ ] **CODE_OF_CONDUCT.md** — Community standards (root or .github/)
- [ ] **SECURITY.md** — Security policy and reporting process
- [ ] **LICENSE** — License file with correct project name

### 2. GitHub Configuration
- [ ] **.github/CODEOWNERS** — Automated reviewer assignment
- [ ] **.github/PULL_REQUEST_TEMPLATE.md** — PR checklist
- [ ] **.github/ISSUE_TEMPLATE/** — Structured issue forms
- [ ] **.gitignore** — File exclusion patterns
- [ ] **.gitattributes** — Merge strategies, line endings

### 3. GitHub Actions Workflows
- [ ] Workflow naming clarity (descriptive, consistent)
- [ ] Permissions scoped minimally (contents, issues, pull-requests)
- [ ] Error handling and fallbacks
- [ ] Concurrency control for resource contention
- [ ] Secrets management (no hardcoded secrets)
- [ ] Conditional execution (avoid unnecessary runs)
- [ ] Pagination for large API responses

### 4. Security & Dependencies
- [ ] **CodeQL** or equivalent security scanning
- [ ] **Dependabot** configured (actions, language-specific packages)
- [ ] Secrets not committed to code
- [ ] Workflow tokens scoped appropriately
- [ ] Security advisory process documented

### 5. Branch Strategy
- [ ] Branch protection rules configured (main, preview, dev)
  - Require pull request before merging
  - Require approvals (1-2 reviewers)
  - Require status checks
  - Dismiss stale approvals
  - Restrict force push and deletion
- [ ] Branch strategy documented (feature → dev → main flow)
- [ ] Merge strategy defined (squash vs. merge by branch)

### 6. Labels & Routing
- [ ] Label system defined (namespace design: type:, priority:, etc.)
- [ ] Label automation (auto-create from team roster or config)
- [ ] Label mutual exclusivity enforced
- [ ] High-signal labels visually distinct (bugs, security, feedback)

## Audit Process

### Step 1: Gather Context
1. Read `.ai-team/routing.md` (if exists) for team structure
2. Read `.ai-team/team.md` (if exists) for squad roster
3. Read `.ai-team/decisions.md` for team conventions
4. Check `CONTRIBUTING.md` for documented standards

### Step 2: Audit Files
Use `glob` and `view` tools to check:
```
glob: **/.github/CODEOWNERS
glob: **/.github/PULL_REQUEST_TEMPLATE.md
glob: **/.github/ISSUE_TEMPLATE/**
glob: **/CONTRIBUTING.md
glob: **/CODE_OF_CONDUCT.md
glob: **/SECURITY.md
glob: **/.gitignore
```

### Step 3: Audit Workflows
List all workflows:
```
view: .github/workflows/
```

For each workflow, check:
- **Naming:** Descriptive and consistent
- **Triggers:** Appropriate (push, PR, schedule, workflow_dispatch)
- **Permissions:** Minimal scope (contents: read, issues: write)
- **Error handling:** try-catch, fallbacks, logging
- **Secrets:** No hardcoded values, use `secrets.*`
- **Concurrency:** Groups defined for resource contention

### Step 4: Security Audit
- [ ] Check for CodeQL workflow (`.github/workflows/codeql*.yml`)
- [ ] Check for Dependabot config (`.github/dependabot.yml`)
- [ ] Verify no secrets in code: `grep -i "password\|secret\|api_key\|token" --exclude-dir=.git`
- [ ] Check workflow permissions (read vs. write)

### Step 5: Branch Protection
⚠️ **Note:** Branch protection rules are GitHub repository settings, not files.  
You cannot read them via tools — document recommended settings and route to repo admin.

**Recommended Settings (main branch):**
- ✅ Require pull request before merging
- ✅ Require approvals: 1-2
- ✅ Dismiss stale approvals when new commits pushed
- ✅ Require status checks: build, test, lint
- ✅ Require conversation resolution
- ✅ Restrict who can push: maintainers only
- ✅ Block force pushes and deletion

### Step 6: Produce Report
Create a decision document with:

**Format:**
```markdown
### {date}: GitHub configuration audit
**By:** {agent_name}
**What:** Comprehensive review of GitHub setup, workflows, documentation, and security
**Why:** Ensure repository is clean, well-documented, and follows best practices

---

## Overall Status: 🟢 Green / 🟡 Yellow / 🔴 Red

{Brief summary of strengths and gaps}

---

## Key Findings by Category

### 1. 🟢/🟡/🔴 {Category Name}
- ✅ Strength: {description}
- ⚠️ Gap: {description}
- 🔴 Critical: {description}

---

## Action Items (Prioritized)

| Priority | Item | Description | Owner | Effort |
|----------|------|-------------|-------|--------|
| 🔴 Critical | {item} | {description} | {owner} | {effort} |
| 🟡 High | {item} | {description} | {owner} | {effort} |
| 🟢 Nice-to-Have | {item} | {description} | {owner} | {effort} |
```

## Report Prioritization

**Use this priority framework:**

### 🔴 Critical (Immediate)
- Missing README or incorrect content
- No .gitignore (risk of secrets/artifacts committed)
- Security vulnerabilities
- Missing CODEOWNERS (if team > 2 people)

### 🟡 High Priority (Next Sprint)
- Missing issue/PR templates
- No branch protection rules
- Incomplete CONTRIBUTING.md
- Workflow security gaps (permissions too broad)

### 🟢 Nice-to-Have (Backlog)
- Documentation location (root vs. .github/)
- Label documentation
- Workflow consolidation/refactoring
- Enhanced automation

## Tips

1. **Read actual files** — don't assume based on conventions
2. **Compare against CONTRIBUTING.md** — use as the source of truth for team standards
3. **Flag deprecated practices** — e.g., actions@v2 instead of @v4
4. **Check for content mismatches** — SECURITY.md referencing wrong project name
5. **Note file locations** — CONTRIBUTING.md in docs/ vs. root
6. **Verify directory paths** — Dependabot paths, workflow artifacts
7. **Prioritize by impact** — security > workflows > documentation > nice-to-haves

## Anti-Patterns to Flag

- **Hardcoded secrets** in workflows or code
- **Overly broad permissions** (write when read is sufficient)
- **No error handling** in workflows (silent failures)
- **Missing pagination** for large API responses
- **No concurrency control** on workflows that modify state
- **Duplicate workflows** with unclear purpose
- **Generic placeholders** in documentation
- **Wrong project names** in documentation files

## Output Hygiene

⚠️ **User-facing report:**
- State WHAT you found and WHY it matters
- Never expose tool internals (SQL queries, schemas)
- Never narrate step-by-step process
- Focus on outcomes and recommendations

## Success Criteria

A good audit report:
- ✅ Identifies all critical gaps
- ✅ Prioritizes by impact (security > workflows > docs)
- ✅ Provides actionable recommendations with effort estimates
- ✅ Assigns owners where appropriate
- ✅ Explains WHY each gap matters (impact statement)
- ✅ Highlights strengths (not just gaps)
- ✅ Is concise and scannable (tables, sections, bullets)

## Related Skills

- **Branch Strategy Design** — Document branch flow and merge strategy
- **Workflow Security Hardening** — Scope permissions, secrets management
- **Issue Template Design** — Create structured issue forms
- **PR Template Design** — Create checklist-based PR templates
- **CODEOWNERS Design** — Map file paths to team members/roles

---

**Last Updated:** 2026-02-19  
**Author:** Elrond (GitHub Ops)
