---
post_title: Squad Team Portability — Quick Start Guide
author1: Aragorn (Lead Developer)
post_slug: squad-team-portability
microsoft_alias: mpaulosky
featured_image: ""
categories: Architecture, Process
tags: squad, team, portability, reusability
ai_note: AI-assisted (GitHub Copilot)
summary: Step-by-step guide for reusing the IssueManager squad team across multiple projects with accumulated experience.
post_date: 2026-03-04
---

# Squad Team Portability — Quick Start Guide

## Overview

This guide explains how to reuse the IssueManager squad team (Aragorn, Gimli, Sam, Boromir, Legolas, Frodo, Gandalf, Scribe, Ralph) across multiple projects while preserving accumulated experience.

**Key Benefits:**
- ✅ Consistent team identity across projects
- ✅ Agents carry cross-project learnings (career.md)
- ✅ Easy installation (one script)
- ✅ Version tracking (know which team you're using)

---

## Prerequisites

1. Create a personal team repository: `github.com/mpaulosky/squad-team`
2. Populate it with IssueManager's `.squad/` files (see Structure below)
3. Extract career learnings from agent history files (see Career Memory below)
4. Write installation scripts (see Installation Scripts below)
5. Tag the team repo: `git tag v0.5.2 -m "IssueManager baseline"`

---

## Team Repository Structure

```
mpaulosky/squad-team/
├── README.md                             # Team overview, version, changelog
├── LICENSE                               # MIT or similar
├── install-squad.ps1                     # PowerShell installation script
├── install-squad.sh                      # Bash installation script
├── .squad/
│   ├── team.md                           # Roster, roles, @copilot profile
│   ├── routing.md                        # Signal → agent routing rules
│   ├── ceremonies.md                     # Ceremonies (pre-sprint, retro, code review)
│   ├── casting/
│   │   ├── registry.json                 # Persistent name mappings
│   │   └── policy.json                   # Universe constraints
│   ├── agents/
│   │   ├── aragorn/
│   │   │   ├── charter.md                # Identity, expertise, responsibilities
│   │   │   └── career.md                 # Cross-project learnings (NEW)
│   │   ├── gimli/
│   │   │   ├── charter.md
│   │   │   └── career.md
│   │   ├── sam/
│   │   │   ├── charter.md
│   │   │   └── career.md
│   │   ├── legolas/
│   │   │   ├── charter.md
│   │   │   └── career.md
│   │   ├── boromir/
│   │   │   ├── charter.md
│   │   │   └── career.md
│   │   ├── frodo/
│   │   │   ├── charter.md
│   │   │   └── career.md
│   │   └── gandalf/
│   │       ├── charter.md
│   │       └── career.md
│   ├── skills/                           # Transferable patterns
│   │   ├── pre-push-test-gate/
│   │   │   └── SKILL.md
│   │   └── build-repair/
│   │       └── SKILL.md
│   └── templates/                        # Blank templates
│       ├── decisions.md.template
│       └── now.md.template
```

**Files NOT in team repo (generated fresh per project):**
- `.squad/decisions.md`
- `.squad/decisions/inbox/`
- `.squad/agents/{name}/history.md`
- `.squad/orchestration-log/`
- `.squad/log/`
- `.squad/identity/now.md`

---

## Installing the Team on a New Project

### Step 1: Clone the Team Repo (One Time)

```powershell
# Clone the team repo to a local directory
cd E:\github\
git clone https://github.com/mpaulosky/squad-team.git
```

### Step 2: Run the Installation Script

```powershell
# Navigate to your new project
cd E:\github\MyNewProject

# Run the installation script
..\squad-team\install-squad.ps1 -ProjectName "MyNewProject" -Stack ".NET 10, Blazor, PostgreSQL"
```

**The script will:**
1. Copy `.squad/team.md`, `routing.md`, `ceremonies.md`, `casting/`, `agents/`, `skills/`
2. Create fresh project-specific files: `decisions.md`, `identity/now.md`, `history.md` (per agent)
3. Update `team.md` with your project name, stack, and repo URL
4. Display next steps

### Step 3: Verify Installation

```powershell
# Check that .squad/ exists
ls .squad

# Verify team version in team.md
cat .squad\team.md | Select-String "Squad version"
```

### Step 4: Connect GitHub Issue Source

1. Open `.squad/team.md`
2. Update the `## Issue Source` section:
   ```markdown
   ## Issue Source
   - **Repository:** mpaulosky/MyNewProject
   - **Connected:** 2026-03-04
   - **Filters:** state: open, label: squad
   ```

### Step 5: Run Build Repair to Establish Baseline

```powershell
# Follow the build-repair prompt
# This establishes zero-error baseline and verifies tests pass
```

---

## Career Memory: What Travels Between Projects

Each agent has a `career.md` file containing **only cross-project learnings** — patterns, anti-patterns, and principles that apply broadly.

### Example Learnings That Travel:

**Aragorn (Lead Developer):**
- Vertical Slice Architecture (VSA) patterns
- CQRS with Result<T>
- Pre-push gate enforcement
- Protected branch guard (no `.squad/` files on `feature/*` branches)
- MSBuild design-time compilation gotchas

**Gimli (Tester):**
- Integration test isolation (`[Collection("Integration")]`)
- DateTime.UtcNow anti-pattern in DTO equality tests
- Mock signature drift when repository interfaces change
- File header conventions (block copyright format)

**Sam (Backend Developer):**
- Repository interface-first design
- MongoDB query patterns
- Minimal API endpoint registration
- Result<T> error handling

### What Does NOT Travel:

- Project-specific bug fixes (e.g., "Fixed IssueRepository.GetAsync null ref")
- One-off CI failures
- Routine PR reviews
- Temporary workarounds

---

## Updating the Team Repo After a Project

After completing a project, extract key learnings:

### Step 1: Review Agent History Files

```powershell
# Read each agent's history from completed project
cat .squad\agents\aragorn\history.md
cat .squad\agents\gimli\history.md
# ... etc.
```

### Step 2: Extract Cross-Project Patterns

Look for:
- Repeatable patterns (e.g., "Always use X for Y")
- Anti-patterns (e.g., "Never do Z because...")
- Framework-specific gotchas (e.g., "MSBuild condition blocks design-time compilation")
- Quality gates (e.g., "Pre-push test gate prevents CI failures")

### Step 3: Update Career Files in Team Repo

```powershell
# Navigate to team repo
cd E:\github\squad-team

# Edit career files
code .squad\agents\aragorn\career.md
code .squad\agents\gimli\career.md
# ... etc.

# Commit changes
git add .squad/agents/*/career.md
git commit -m "Post-IssueManager career learnings"
```

### Step 4: Tag and Push

```powershell
# Tag the new version
git tag v0.5.3 -m "Post-IssueManager learnings"

# Push to GitHub
git push origin main --tags
```

---

## Versioning & Upgrades

### Checking Team Version

Each project's `.squad/team.md` shows the squad version:

```markdown
- **Squad version:** v0.5.2
```

### Upgrading an Existing Project

If the team repo has new learnings and you want to update an existing project:

```powershell
# Back up current squad state
Copy-Item -Recurse .squad .squad.backup

# Run installation script (overwrites team files, preserves project-specific files)
..\squad-team\install-squad.ps1 -ProjectName "MyProject" -Stack ".NET 10, Blazor, MongoDB"

# Review diff
git diff .squad/

# Restore any accidentally overwritten project-specific content
# (decisions.md, history.md, etc. should be preserved by script logic)
```

---

## FAQ

### Q: Can I customize the team for a specific project?

**A:** Yes. After installation, edit `.squad/routing.md` or agent charters in the project. Those changes stay in the project and won't affect the team repo.

### Q: What if my new project uses a different stack (e.g., PostgreSQL instead of MongoDB)?

**A:** Update agent charters in the project to reflect the new stack. For example, update Sam's `charter.md` to say "PostgreSQL + Entity Framework Core" instead of "MongoDB". The core routing logic and ceremonies still apply.

### Q: Can I add new agents to the team?

**A:** Yes. Add a new agent directory in the team repo (`.squad/agents/newagent/charter.md`, `career.md`), update `team.md` and `routing.md`, and re-run the installation script on projects that need the new agent.

### Q: What if I want to use a different universe (e.g., Star Wars instead of LOTR)?

**A:** Update `.squad/casting/registry.json` and rename agent directories. The team repo structure stays the same; only names change.

### Q: How do I know if the installation script preserved my project-specific files?

**A:** The script creates fresh `decisions.md`, `history.md`, and `now.md` files. If you have existing content, back up `.squad/` before running the script. The script should check for existing files and prompt before overwriting (see script logic in design document).

---

## Troubleshooting

### Installation script fails with "already exists" error

**Solution:** Back up `.squad/`, delete it, and re-run the script. Or edit the script to skip the overwrite check.

### Team version in team.md doesn't match team repo tag

**Solution:** Re-run `install-squad.ps1` to pull the latest version from the team repo. Or manually update `team.md` with the correct version tag.

### Career files are empty after installation

**Solution:** Career files must be populated in the team repo BEFORE installation. Extract learnings from IssueManager history files and commit to team repo first.

---

## Quick Reference Commands

```powershell
# Install team on new project
..\squad-team\install-squad.ps1 -ProjectName "MyProject" -Stack ".NET, Blazor, MongoDB"

# Check team version
cat .squad\team.md | Select-String "Squad version"

# Update team repo after project
cd E:\github\squad-team
git add .squad/agents/*/career.md
git commit -m "Post-ProjectName learnings"
git tag v0.5.3 -m "Post-ProjectName"
git push origin main --tags

# Upgrade existing project to newer team version
Copy-Item -Recurse .squad .squad.backup
..\squad-team\install-squad.ps1
git diff .squad/
```

---

## Related Documentation

- Full design decision: `.squad/decisions/inbox/aragorn-team-portability-design.md`
- Build repair prompt: `.github/prompts/build-repair.prompt.md`
- Pre-push test gate skill: `.squad/skills/pre-push-test-gate/SKILL.md`

---

**Last Updated:** 2026-03-04  
**Team Version:** v0.5.2  
**Maintained by:** Aragorn (Lead Developer)
