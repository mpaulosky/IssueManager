#!/usr/bin/env bash
# Repo-specific checks for the local gate, called by scripts/gate.sh after its
# lints and before it builds, so they run in pre-push, in any sandbox that
# runs the gate, and by hand.
#
#   gate-checks.sh <merge-base>
#
# <merge-base> is where this branch left origin/main, or empty when there is
# no origin/main; in that case treat every tracked file as changed. Exit
# non-zero to fail the gate.
#
# scripts/gate.sh is Owned by the repo-ci-baseline Template and is overwritten
# on every Apply; this file is Seed, so it belongs to the repo. Put checks only
# this repo needs here, each guarded by the paths it covers, for example:
#
#   if git diff --quiet "$base" HEAD -- src/Tool; then :; else pnpm run check:tool; fi
set -euo pipefail

base="${1-}"
: "$base"

# Sandcastle's type check and tests (pnpm run test:sandcastle), when this
# branch changes Sandcastle, the root Node packages it runs on, or the branch
# and PR title scripts its tests run its branch names and PR titles through.
# Without a merge base every file counts as changed. CI doesn't run this file,
# so the pre-push gate and .sandcastle/check.sh are where those tests run.
sandcastle_paths=(.sandcastle package.json pnpm-lock.yaml pnpm-workspace.yaml scripts/check-branch-name.sh scripts/check-pr-title.sh)
if [[ -z "$base" ]] || ! git diff --quiet "$base" HEAD -- "${sandcastle_paths[@]}"; then
  echo "Sandcastle changed: running pnpm run test:sandcastle"
  pnpm install --frozen-lockfile --silent
  pnpm run test:sandcastle
fi
