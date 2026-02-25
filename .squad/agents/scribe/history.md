# Scribe — History

## Core Context
Silent session logger for IssueManager squad. Writes orchestration logs, session logs, merges decision inbox, commits .squad/ state. Model: always claude-haiku-4.5.

## Learnings

### Branching rule for commits
- Only commit `.squad/` changes on `squad/*` branches
- NEVER commit on `feature/*` branches (Protected Branch Guard will block the PR)
- NEVER commit directly to main

### Drop-box pattern
- Agents write decisions to `.squad/decisions/inbox/{agent-name}-{slug}.md`
- Scribe merges inbox → `decisions.md` and deletes inbox files
- Never edit decisions.md directly — append only
