import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { chmodSync, existsSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { describe, it } from "node:test";
import {
  branchFor,
  commitsAhead,
  containsBase,
  headOf,
  isIssueBranch,
  mergeBase,
  parseHeads,
  prepareBranches,
  pushChecked,
  slugFor,
  uniqueIssues,
  withoutOpenPullRequests,
} from "./branches.mts";

const issue = (number: number, title: string, labels: string[] = ["Sandcastle"]) => ({ number, title, labels });

// The branch standard, from the script the pre-push hook and CI share.
const passesBranchStandard = (branch: string) => {
  try {
    execFileSync("bash", ["scripts/check-branch-name.sh", branch], { stdio: "ignore" });
    return true;
  } catch {
    return false;
  }
};

describe("slugFor", () => {
  it("drops the conventional-commit prefix, lower-cases and joins words with hyphens", () => {
    assert.equal(
      slugFor("feat(sandcastle): Hold back issues whose blockers haven't landed"),
      "hold-back-issues-whose-blockers-havent-landed",
    );
  });

  it("drops prefixes without a scope or with a breaking-change mark", () => {
    assert.equal(slugFor("fix: Stop the crash"), "stop-the-crash");
    assert.equal(slugFor("refactor(Domain)!: Rename Result"), "rename-result");
  });

  it("drops curly apostrophes too", () => {
    assert.equal(slugFor("Don’t reuse the cache"), "dont-reuse-the-cache");
  });

  it("joins every run of other characters into one hyphen and trims the ends", () => {
    assert.equal(slugFor("  Add  C# / .NET 10 support!  "), "add-c-net-10-support");
  });

  it("cuts a long title at a hyphen within 50 characters", () => {
    const slug = slugFor("Make the planner hold back every issue whose blockers have not landed on main yet");
    assert.equal(slug, "make-the-planner-hold-back-every-issue-whose");
    assert.ok(slug.length <= 50);
  });

  it("falls back to 'issue' when the title has no letters or digits", () => {
    assert.equal(slugFor("feat: ???"), "issue");
  });
});

describe("isIssueBranch", () => {
  it("matches the issue's feature, fix and hotfix branches, not another issue's", () => {
    assert.ok(isIssueBranch("feature/4-add-search", 4));
    assert.ok(isIssueBranch("fix/4-stop-the-crash", 4));
    assert.ok(isIssueBranch("hotfix/4-stop-the-crash", 4));
    assert.ok(!isIssueBranch("feature/42-add-search", 4));
    assert.ok(!isIssueBranch("fix/42-stop-the-crash", 4));
    assert.ok(!isIssueBranch("chore/4-add-search", 4));
  });
});

describe("branchFor", () => {
  it("names a bug's branch fix/{n}-{slug}", () => {
    assert.equal(branchFor(issue(7, "fix: Stop the crash", ["Sandcastle", "bug"]), []), "fix/7-stop-the-crash");
  });

  it("names any other issue's branch feature/{n}-{slug}", () => {
    assert.equal(branchFor(issue(8, "feat: Add search"), []), "feature/8-add-search");
  });

  it("reuses the issue's existing branch after its title or labels change", () => {
    const existing = ["feature/80-other-work", "feature/8-add-search"];
    assert.equal(branchFor(issue(8, "feat: Add full-text search", ["Sandcastle", "bug"]), existing), "feature/8-add-search");
  });

  it("reuses an existing fix/ or hotfix/ branch", () => {
    assert.equal(branchFor(issue(9, "Add search"), ["fix/9-stop-the-crash"]), "fix/9-stop-the-crash");
    assert.equal(branchFor(issue(9, "Add search", ["bug"]), ["hotfix/9-urgent"]), "hotfix/9-urgent");
  });

  it("only names branches that pass the branch standard", () => {
    for (const title of ["feat: Add search", "fix: Stop the crash!", "???", "Don’t reuse the cache"]) {
      for (const labels of [["Sandcastle"], ["Sandcastle", "bug"]]) {
        const branch = branchFor(issue(12, title, labels), []);
        assert.ok(passesBranchStandard(branch), `${branch} fails scripts/check-branch-name.sh`);
      }
    }
  });
});

describe("withoutOpenPullRequests", () => {
  it("holds back issues with an open PR from one of their branches", () => {
    const issues = [issue(1, "One"), issue(2, "Two"), issue(3, "Three")];
    const { ready, inReview } = withoutOpenPullRequests(issues, ["fix/2-two", "feature/30-other", "chore/tidy"]);
    assert.deepEqual(ready.map((i) => i.number), [1, 3]);
    assert.deepEqual(inReview.map((i) => i.number), [2]);
  });
});

describe("parseHeads", () => {
  it("strips refs/heads/ from ls-remote output", () => {
    assert.deepEqual(parseHeads("abc\trefs/heads/feature/1-a\ndef\trefs/heads/fix/2-b\n"), ["feature/1-a", "fix/2-b"]);
  });
});

describe("uniqueIssues", () => {
  it("keeps each issue once, in first-seen order", () => {
    const issues = [issue(2, "Two"), issue(1, "One"), issue(2, "Two again")];
    assert.deepEqual(uniqueIssues(issues).map((i) => i.title), ["Two", "One"]);
  });
});

describe("prepareBranches", () => {
  it("fetches only the branches that already exist on origin", () => {
    const fetched: string[] = [];
    const work = prepareBranches([issue(1, "Add search"), issue(2, "Stop the crash", ["bug"])], {
      remoteBranches: () => ["feature/1-add-search"],
      localBranches: () => [],
      fetch: (branch) => fetched.push(branch),
    });
    assert.deepEqual(work.map((w) => w.branch), ["feature/1-add-search", "fix/2-stop-the-crash"]);
    assert.deepEqual(fetched, ["feature/1-add-search"]);
  });

  it("reuses a local branch that was never pushed", () => {
    const fetched: string[] = [];
    const work = prepareBranches([issue(3, "Renamed title")], {
      remoteBranches: () => [],
      localBranches: () => ["fix/3-old-title"],
      fetch: (branch) => fetched.push(branch),
    });
    assert.deepEqual(work.map((w) => w.branch), ["fix/3-old-title"]);
    assert.deepEqual(fetched, []);
  });
});

// The real git commands, in a clone of a throwaway origin.
describe("worktree git", () => {
  const setup = () => {
    const root = mkdtempSync(join(tmpdir(), "branches-test-"));
    const origin = join(root, "origin.git");
    const clone = join(root, "clone");
    const git = (cwd: string, ...args: string[]) =>
      execFileSync("git", args, { cwd, encoding: "utf8", stdio: ["ignore", "pipe", "ignore"] }).trim();
    git(root, "init", "-q", "--bare", "-b", "main", origin);
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
    return { root, clone, git, commit, cleanup: () => rmSync(root, { recursive: true, force: true }) };
  };

  it("counts the commits main lacks, and reads HEAD", () => {
    const repo = setup();
    try {
      const sha = repo.commit("a.txt", "a\n");
      assert.equal(commitsAhead(repo.clone, sha), 1);
      assert.deepEqual(headOf(repo.clone), { sha, branch: "feature/1-work" });
    } finally {
      repo.cleanup();
    }
  });

  it("merges a moved main into the branch", () => {
    const repo = setup();
    try {
      repo.commit("a.txt", "a\n");
      repo.git(repo.clone, "checkout", "-q", "main");
      repo.commit("b.txt", "b\n");
      repo.git(repo.clone, "push", "-q", "origin", "main");
      repo.git(repo.clone, "fetch", "-q", "origin");
      repo.git(repo.clone, "checkout", "-q", "feature/1-work");
      assert.equal(containsBase(repo.clone, "HEAD"), false);
      assert.equal(mergeBase(repo.clone), true);
      assert.equal(containsBase(repo.clone, "HEAD"), true);
    } finally {
      repo.cleanup();
    }
  });

  it("undoes a conflicting merge of main", () => {
    const repo = setup();
    try {
      const before = repo.commit("README.md", "branch\n");
      repo.git(repo.clone, "checkout", "-q", "main");
      repo.commit("README.md", "main\n");
      repo.git(repo.clone, "push", "-q", "origin", "main");
      repo.git(repo.clone, "fetch", "-q", "origin");
      repo.git(repo.clone, "checkout", "-q", "feature/1-work");
      assert.equal(mergeBase(repo.clone), false);
      assert.equal(repo.git(repo.clone, "rev-parse", "HEAD"), before);
      assert.equal(repo.git(repo.clone, "status", "--porcelain"), "");
    } finally {
      repo.cleanup();
    }
  });

  it("pushes the checked commit, not the branch's later one, without running the worktree's hooks", () => {
    const repo = setup();
    try {
      const checked = repo.commit("a.txt", "a\n");
      repo.commit("b.txt", "unchecked\n");
      const hooks = join(repo.clone, "hooks");
      mkdirSync(hooks);
      const marker = join(repo.root, "hook-ran");
      writeFileSync(join(hooks, "pre-push"), `#!/bin/sh\ntouch '${marker}'\n`);
      chmodSync(join(hooks, "pre-push"), 0o755);
      repo.git(repo.clone, "config", "core.hooksPath", hooks);

      pushChecked(repo.clone, checked, "feature/1-work");

      assert.equal(repo.git(repo.clone, "ls-remote", "origin", "refs/heads/feature/1-work").split("\t")[0], checked);
      assert.equal(existsSync(marker), false);
    } finally {
      repo.cleanup();
    }
  });

  it("refuses to push over commits on origin the branch lacks", () => {
    const repo = setup();
    try {
      repo.commit("a.txt", "a\n");
      repo.git(repo.clone, "push", "-q", "origin", "feature/1-work");
      repo.commit("b.txt", "b\n");
      repo.git(repo.clone, "push", "-q", "origin", "feature/1-work");
      const older = repo.git(repo.clone, "rev-parse", "HEAD~1");
      assert.throws(() => pushChecked(repo.clone, older, "feature/1-work"));
    } finally {
      repo.cleanup();
    }
  });
});
