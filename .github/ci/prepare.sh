#!/usr/bin/env bash
# Repo-specific CI setup, called by ci.yml after it restores and before it builds.
#
#   prepare.sh build              in the "Build Solution" job
#   prepare.sh test <test-name>   in each "Tests: <test-name>" matrix job
#
# ci.yml is Owned by the repo-ci-baseline Template and is overwritten on every
# Apply; this file is Seed, so it belongs to the repo. Put what only this repo
# needs here: tools the build runs (for example `corepack enable` for pnpm),
# images to pull or projects to publish before a test project runs. To pass
# environment variables to the later steps, append NAME=value lines to
# "$GITHUB_ENV".
#
# The test name comes from a file name in the PR, so treat it as data: quote
# it and never eval it.
set -euo pipefail

job="${1:?usage: prepare.sh build|test [test-name]}"
test_name="${2:-}"

# Building src/Web runs the Tailwind CSS build through pnpm (its csproj runs
# `pnpm install --frozen-lockfile` itself when node_modules is missing), and
# the Bunit and E2E test projects build it too. Corepack provides the pnpm
# version pinned by "packageManager" in src/Web/package.json; the build step
# that first runs pnpm comes later, so it gets the no-prompt setting too.
enable_pnpm() {
  export COREPACK_ENABLE_DOWNLOAD_PROMPT=0
  echo "COREPACK_ENABLE_DOWNLOAD_PROMPT=0" >> "${GITHUB_ENV:-/dev/null}"
  corepack enable
}

# The E2E host serves the web app over HTTPS with the ASP.NET Core dev
# certificate, and the AppHost's health check (and the web app's calls to the
# API) reject it unless it's trusted. On Linux, --trust exports it to
# ~/.aspnet/dev-certs/trust, which OpenSSL reads only through SSL_CERT_DIR.
trust_dev_cert() {
  # Exit code 4 is partial trust: OpenSSL (what .NET reads) trusts the
  # certificate, but the runner has no browser store to add it to. Playwright
  # ignores HTTPS errors, so that's enough.
  local status=0
  dotnet dev-certs https --trust || status=$?
  if (( status != 0 && status != 4 )); then
    return "$status"
  fi
  local trust_dir="$HOME/.aspnet/dev-certs/trust"
  local system_dir
  system_dir="$(openssl version -d | sed -E 's/^OPENSSLDIR: "(.*)"$/\1/')/certs"
  echo "SSL_CERT_DIR=${trust_dir}:${system_dir}" >> "${GITHUB_ENV:-/dev/null}"
}

case "$job" in
  build) enable_pnpm ;;
  test)
    enable_pnpm
    if [[ "$test_name" == "AppHost.Tests.E2E" ]]; then
      trust_dev_cert
    fi
    ;;
  *) echo "prepare.sh: unknown job '$job'" >&2; exit 2 ;;
esac
