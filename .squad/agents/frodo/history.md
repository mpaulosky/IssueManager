# Frodo — History

## Core Context
Tech Writer on IssueManager (.NET 10, XML docs, Markdown). User: Matthew Paulosky.

## Learnings

### Project documentation structure
- `docs/` — working documents, build logs, guides, reviews
- `README.md` — root-level, user-facing
- `CONTRIBUTING.md` — contribution guide (content in `docs/`, root may be symlink/reference)
- XML docs required on all public API types and members
- File header required: `// Copyright (c) 2026. All rights reserved.`

### Build repair documentation
- Build repair runs documented in `docs/build-repair-log.md` or `docs/build-log.txt`
- Format: date, errors found, resolutions, final status

### Squad skills
- Created `copilot-sdk-csharp-usage` skill as replacement for removed `.github/instructions/copilot-sdk-csharp.instructions.md`
- Skills are on-demand reference material; instruction files load globally on every .cs file
- Format: Overview → When to Use → Key Patterns → Best Practices → Gotchas → References
