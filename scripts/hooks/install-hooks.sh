#!/usr/bin/env bash
# install-hooks.sh — copies git hooks from scripts/hooks/ to .git/hooks/ and makes them executable.
#
# Usage: bash scripts/hooks/install-hooks.sh
# Run from the repository root.

set -euo pipefail

HOOKS_DIR="$(git rev-parse --git-dir)/hooks"
SOURCE_DIR="scripts/hooks"

echo "Installing git hooks from $SOURCE_DIR → $HOOKS_DIR"

for hook in "$SOURCE_DIR"/*; do
  name=$(basename "$hook")
  dest="$HOOKS_DIR/$name"
  cp "$hook" "$dest"
  chmod +x "$dest"
  echo "✅ Installed $name"
done

echo ""
echo "All hooks installed. They will run automatically on relevant git operations."
