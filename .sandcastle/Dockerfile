FROM node:22-bookworm

# Install system dependencies. python3 runs .github/scripts/discover_tests.py
# for .sandcastle/check.sh; libicu is the .NET SDK's globalization dependency.
# There is no Docker, on purpose: the host's Docker socket would give the
# agents root on the host. check.sh skips the test projects that need Docker.
RUN apt-get update && apt-get install -y \
  git \
  curl \
  jq \
  python3 \
  libicu72 \
  && rm -rf /var/lib/apt/lists/*

# Install the .NET SDK .sandcastle/check.sh builds and tests with.
# global.json pins 10.0.x with rollForward latestMinor, so the latest GA SDK
# on the 10.0 channel satisfies it, and keeps satisfying it as Dependabot
# bumps global.json within 10.0. Rebuild the image to pick up a newer SDK;
# change the channel when global.json moves to a new major version.
ARG DOTNET_CHANNEL=10.0
ENV DOTNET_ROOT=/usr/share/dotnet \
  DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  DOTNET_NOLOGO=1
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
  && bash /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --quality GA --install-dir "$DOTNET_ROOT" \
  && ln -s "$DOTNET_ROOT/dotnet" /usr/local/bin/dotnet \
  && rm /tmp/dotnet-install.sh

# Enable pnpm through corepack. The version comes from package.json's
# packageManager field and is fetched on first use. A current corepack knows
# the keys that sign current pnpm releases; the one bundled with Node may not.
ENV COREPACK_ENABLE_DOWNLOAD_PROMPT=0
RUN npm install -g corepack@latest && corepack enable pnpm

# No GitHub CLI: the sandbox gets no GitHub token. The host reads issues and
# opens pull requests itself (lib/github.mts).

# Build-args for UID/GID alignment: sandcastle docker build-image
# defaults these to the host user's UID/GID so image-built files
# and bind-mounted files share an owner without runtime chown.
ARG AGENT_UID=1000
ARG AGENT_GID=1000

# Rename the base image's "node" user to "agent" and align UID/GID.
RUN groupmod -o -g $AGENT_GID node && usermod -o -u $AGENT_UID -g $AGENT_GID -d /home/agent -m -l agent node
USER ${AGENT_UID}:${AGENT_GID}

# Install Claude Code CLI
RUN curl -fsSL https://claude.ai/install.sh | bash

# Add Claude to PATH
ENV PATH="/home/agent/.local/bin:$PATH"

WORKDIR /home/agent

# In worktree sandbox mode, Sandcastle bind-mounts the git worktree at /home/agent/workspace
# and overrides the working directory to /home/agent/workspace at container start.
# Structure your Dockerfile so that /home/agent/workspace can serve as the project root.
ENTRYPOINT ["sleep", "infinity"]
