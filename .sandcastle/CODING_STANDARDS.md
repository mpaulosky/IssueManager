# Coding Standards

The reviewer loads this file during code review. The repository's own guidance is the source of truth, so read it
rather than relying on this summary:

- @.github/copilot-instructions.md: project context, working agreements, .NET, Blazor, data, testing and validation
- @.github/instructions/dotnet-project.instructions.md: .NET project conventions
- @.github/instructions/blazor.instructions.md: Razor components, code-behind and component CSS
- @.github/instructions/markdown.instructions.md: documentation
- @.github/instructions/git-commit-instructions.md: commit messages
- `.editorconfig`: formatting

When this summary and those files disagree, those files win.

## Style

- Target .NET 10 with the C# version set in `Directory.Build.props`. Nullable reference types, analyzers and
  warnings-as-errors are on; fix a warning rather than suppress it.
- Follow `.editorconfig`: tabs in C# and Razor, two-space indentation in Markdown and YAML, LF line endings.
- Document public types and members with XML `/// <summary>` comments.
- Packages are managed centrally in `Directory.Packages.props`; make the smallest justified package change.
- No empty catch blocks and no `Thread.Sleep` in production code. Handle an exception deliberately, or log it with
  context and rethrow.
- Never put secrets, credentials, connection strings or tokens in source, configuration, tests or documentation.

## Testing

- xUnit v3 with FluentAssertions and NSubstitute, following the nearby tests' conventions.
- Write new behaviour and bug fixes test-first where practical. Keep the `// Arrange`, `// Act` and `// Assert`
  markers in test methods.
- Never change the code under test just to make a test pass; fix the implementation or the test to match the
  intended behaviour.
- The gate is `bash scripts/gate.sh`: it lints the changed Markdown, YAML, workflow and shell files, builds the
  solution with warnings as errors, and runs every test project.

## Architecture

- Keep to the existing domain, CQRS, repository and dependency-injection boundaries; UI code doesn't reach past
  them.
- Keep MongoDB access behind the infrastructure abstractions, and build filters with the driver's typed APIs.
- Treat schema and persistence changes as compatibility-sensitive: add focused tests and note any migration.
- Reuse existing Blazor components, Tailwind CSS v4 styles and accessibility patterns before adding new ones.
