import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { chmodSync, existsSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { describe, it } from "node:test";
import {
  commitsAhead,
  containsBase,
  fetchFromOrigin,
  parseHeads,
  pushChecked,
  syncLocalBranch,
  worktreeHead,
  type Repos,
} from "./git.mts";

describe("parseHeads", () => {
  it("strips refs/heads/ from ls-remote output", () => {
    assert.deepEqual(parseHeads("abc\trefs/heads/feature/1-a\ndef\trefs/heads/fix/2-b\n"), ["feature/1-a", "fix/2-b"]);
  });
});

// The real git commands: a throwaway origin, a host-only bare repo pointing at
// it, and a clone whose config an agent could have written.
describe("host git", () => {
  const setup = () => {
    const root = mkdtempSync(join(tmpdir(), "git-test-"));
    const origin = join(root, "origin.git");
    const clone = join(root, "clone");
    const host = join(root, "host.git");
    const git = (cwd: string, ...args: string[]) =>
      execFileSync("git", args, { cwd, encoding: "utf8", stdio: ["ignore", "pipe", "ignore"] }).trim();
    git(root, "init", "-q", "--bare", "-b", "main", origin);
    git(root, "init", "-q", "--bare", host);
    git(host, "remote", "add", "origin", origin);
    git(root, "clone", "-q", origin, clone);
    git(clone, "config", "user.email", "test@example.com");
    git(clone, "config", "user.name", "Test");
    const commit = (file: string, text: string) => {
      writeFileSync(join(clone, file), text);
      git(clone, "add", file);
      git(clone, "commit", "-q", "-m", `chore: Write ${file}`);
      return git(clone, "rev-parse", "HEAD");
    };
    commit("README.md", "x\n");
    git(clone, "push", "-q", "origin", "main");
    git(clone, "checkout", "-q", "-b", "feature/1-work");
    // A hook the agent planted in the clone, which must never run.
    const marker = join(root, "hook-ran");
    const hooks = join(root, "hooks");
    mkdirSync(hooks);
    for (const hook of ["pre-push", "reference-transaction", "post-merge"]) {
      writeFileSync(join(hooks, hook), `#!/bin/sh\ntouch '${marker}'\n`);
      chmodSync(join(hooks, hook), 0o755);
    }
    const plantHooks = () => git(clone, "config", "core.hooksPath", hooks);
    const repos: Repos = { host, clone };
    return { root, origin, clone, repos, git, commit, marker, plantHooks, cleanup: () => rmSync(root, { recursive: true, force: true }) };
  };

  it("counts the commits the base lacks, and reads HEAD", () => {
    const repo = setup();
    try {
      const base = repo.git(repo.clone, "rev-parse", "main");
      const sha = repo.commit("a.txt", "a\n");
      assert.equal(commitsAhead(repo.clone, base, sha), 1);
      assert.equal(containsBase(repo.clone, base, sha), true);
      assert.deepEqual(worktreeHead(repo.clone), { sha, branch: "feature/1-work" });
    } finally {
      repo.cleanup();
    }
  });

  it("fetches origin's branch through the host repo without running the clone's hooks", () => {
    const repo = setup();
    try {
      const main = repo.git(repo.clone, "rev-parse", "main");
      repo.plantHooks();
      assert.equal(fetchFromOrigin(repo.repos, "main"), main);
      assert.equal(repo.git(repo.clone, "rev-parse", "refs/remotes/origin/main"), main);
      assert.equal(existsSync(repo.marker), false);
    } finally {
      repo.cleanup();
    }
  });

  it("doesn't run an alternate-refs command an agent planted in the clone", () => {
    const repo = setup();
    try {
      // An alternate object store and a command git would run to list its refs.
      mkdirSync(join(repo.clone, ".git", "x", "objects", "info"), { recursive: true });
      mkdirSync(join(repo.clone, ".git", "x", "refs"), { recursive: true });
      writeFileSync(join(repo.clone, ".git", "objects", "info", "alternates"), "../x/objects\n");
      repo.git(repo.clone, "config", "core.alternateRefsCommand", `touch '${repo.marker}'; true`);
      // Origin moves on, so the fetch has something to negotiate.
      repo.git(repo.clone, "checkout", "-q", "main");
      repo.commit("c.txt", "c\n");
      repo.git(repo.clone, "push", "-q", "origin", "main");
      repo.git(repo.clone, "reset", "-q", "--hard", "HEAD~1");
      repo.git(repo.clone, "update-ref", "refs/remotes/origin/main", "HEAD");

      fetchFromOrigin(repo.repos, "main");

      assert.equal(existsSync(repo.marker), false);
    } finally {
      repo.cleanup();
    }
  });

  it("pushes the checked commit, not the branch's later one, to the host repo's origin", () => {
    const repo = setup();
    try {
      const checked = repo.commit("a.txt", "a\n");
      repo.commit("b.txt", "unchecked\n");
      repo.plantHooks();
      // An agent pointing the clone's origin elsewhere changes nothing.
      repo.git(repo.clone, "remote", "set-url", "origin", join(repo.root, "elsewhere.git"));

      pushChecked(repo.repos, checked, "feature/1-work");

      assert.equal(repo.git(repo.origin, "rev-parse", "refs/heads/feature/1-work"), checked);
      assert.equal(existsSync(repo.marker), false);
    } finally {
      repo.cleanup();
    }
  });

  it("refuses to push over commits on origin the branch lacks", () => {
    const repo = setup();
    try {
      repo.commit("a.txt", "a\n");
      repo.commit("b.txt", "b\n");
      repo.git(repo.clone, "push", "-q", "origin", "feature/1-work");
      const older = repo.git(repo.clone, "rev-parse", "HEAD~1");
      repo.git(repo.clone, "reset", "-q", "--hard", older);
      assert.throws(() => pushChecked(repo.repos, older, "feature/1-work"));
    } finally {
      repo.cleanup();
    }
  });

  it("creates a missing local branch from origin's, and fast-forwards one that's behind", () => {
    const repo = setup();
    try {
      repo.commit("a.txt", "a\n");
      const tip = repo.commit("b.txt", "b\n");
      repo.git(repo.clone, "push", "-q", "origin", "feature/1-work", "feature/1-work:feature/2-more");
      repo.git(repo.clone, "checkout", "-q", "main");
      repo.git(repo.clone, "branch", "-f", "feature/1-work", "HEAD");

      for (const branch of ["feature/1-work", "feature/2-more"]) {
        fetchFromOrigin(repo.repos, branch);
        syncLocalBranch(repo.repos, branch);
        assert.equal(repo.git(repo.clone, "rev-parse", `refs/heads/${branch}`), tip, branch);
      }
    } finally {
      repo.cleanup();
    }
  });

  it("leaves a local branch alone when it has commits origin lacks", () => {
    const repo = setup();
    try {
      repo.commit("a.txt", "a\n");
      repo.git(repo.clone, "push", "-q", "origin", "feature/1-work");
      const local = repo.commit("b.txt", "local only\n");
      repo.git(repo.clone, "checkout", "-q", "main");
      fetchFromOrigin(repo.repos, "feature/1-work");
      syncLocalBranch(repo.repos, "feature/1-work");
      assert.equal(repo.git(repo.clone, "rev-parse", "refs/heads/feature/1-work"), local);
    } finally {
      repo.cleanup();
    }
  });
});
