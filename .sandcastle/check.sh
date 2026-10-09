#!/usr/bin/env bash
# The check Sandcastle's agents run before they commit or finish, and the host
# runs in the sandbox before it counts an issue complete: the exit code
# decides, never what an agent reports.
#
# It is scripts/gate.sh without Docker. The sandbox deliberately has no Docker
# (the host's Docker socket would give its agents root on the host), so this
# skips the test projects that need it: any that references Testcontainers or
# Aspire.Hosting.Testing. The host's pre-push gate (scripts/gate.sh) runs
# those before anything is pushed, and CI runs every test project.
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

GREEN='\033[0;32m'; CYAN='\033[0;36m'; RESET='\033[0m'
step() { echo -e "\n${CYAN}▶ $1${RESET}"; }

# The same base and changed files as scripts/gate.sh: everything since this
# branch left origin/main, or every tracked file without an origin/main.
if BASE="$(git merge-base HEAD origin/main 2>/dev/null)"; then
  CHANGED="$(git diff --name-only --diff-filter=ACMR "$BASE" HEAD)"
else
  BASE=""
  CHANGED="$(git ls-files)"
fi

# Of the gate's lints only Markdown runs here: the image has pnpm, but not
# yamllint, actionlint, zizmor or shellcheck (whose fallbacks need Docker).
# The pre-push gate and CI's lint workflows run those.
mapfile -t MD_FILES < <(grep -E '\.md$' <<< "$CHANGED" | grep -Ev '^docs/blogs/' || true)
step "Markdown lint (${#MD_FILES[@]} changed file(s))"
if [[ ${#MD_FILES[@]} -gt 0 ]]; then
  if [[ -x node_modules/.bin/markdownlint-cli2 ]]; then
    node_modules/.bin/markdownlint-cli2 "${MD_FILES[@]}"
  else
    # Keep in step with MARKDOWNLINT_CLI2_VERSION in scripts/gate.sh.
    pnpm dlx "markdownlint-cli2@0.23.3" "${MD_FILES[@]}"
  fi
fi

if [[ -f .github/ci/gate-checks.sh ]]; then
  step "Repo checks (.github/ci/gate-checks.sh)"
  # check.sh runs the Sandcastle tests itself, below: a branch cut before
  # gate-checks.sh ran them would otherwise skip them.
  SANDCASTLE_CHECK=1 bash .github/ci/gate-checks.sh "$BASE"
fi

step "Build"
mapfile -t SOLUTIONS < <(find . -maxdepth 1 -name '*.slnx')
dotnet build "${SOLUTIONS[@]}" --configuration Release -warnaserror

# A project needs Docker when it references Testcontainers (a container per
# test run) or Aspire.Hosting.Testing (the AppHost, which starts containers).
needs_docker() {
  grep -Eq '<PackageReference[^>]+Include="(Testcontainers(\.[^"]*)?|Aspire\.Hosting\.Testing)"' "$1"
}

step "Tests (without Docker)"
# The test projects CI's discover_tests.py finds, as scripts/gate.sh runs them.
TEST_LIST="$(python3 .github/scripts/discover_tests.py --list)"
mapfile -t TEST_PROJECTS < <(grep . <<< "$TEST_LIST" || true)
ran=0
for project in "${TEST_PROJECTS[@]}"; do
  if needs_docker "$project"; then
    echo "Skipping ${project}: it needs Docker. The pre-push gate and CI run it."
    continue
  fi
  dotnet test "$project" --configuration Release
  ran=$((ran + 1))
done
echo "Ran ${ran} of ${#TEST_PROJECTS[@]} test project(s)."

step "Sandcastle tests"
pnpm run test:sandcastle

echo -e "\n${GREEN}✅ Sandcastle check passed.${RESET}"
