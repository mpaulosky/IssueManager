#!/usr/bin/env bash
# install-hooks.sh — copies git hooks from scripts/hooks/ into .git/hooks/
# Run once after cloning: bash scripts/hooks/install-hooks.sh

set -euo pipefail

HOOKS_SRC="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HOOKS_DEST="$(git rev-parse --show-toplevel)/.git/hooks"

install_hook() {
  local name="$1"
  cp "$HOOKS_SRC/$name" "$HOOKS_DEST/$name"
  chmod +x "$HOOKS_DEST/$name"
  echo "✅ Installed $name"
}

echo "Installing git hooks…"
install_hook pre-push
echo "Done. Hooks active in .git/hooks/"
