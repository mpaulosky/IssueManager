import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { mergeCandidates } from "./rounds.mts";

const ahead = (commit: string) => commit !== "at-base";
const none = new Set<string>();

describe("mergeCandidates", () => {
  it("merges a checked commit ahead of the base, even with no commits this round", () => {
    const result = { commits: 0, complete: true, checked: "abc" };
    assert.deepEqual(mergeCandidates([result], ahead, none), { toMerge: [result], progress: true });
  });

  it("doesn't offer a commit twice, and stops when nothing else happened", () => {
    const result = { commits: 0, complete: true, checked: "abc" };
    assert.deepEqual(mergeCandidates([result], ahead, new Set(["abc"])), { toMerge: [], progress: false });
  });

  it("doesn't re-offer a commit while another pipeline makes progress", () => {
    const refused = { commits: 0, complete: true, checked: "abc" };
    const partial = { commits: 2, complete: false };
    assert.deepEqual(mergeCandidates([refused, partial], ahead, new Set(["abc"])), { toMerge: [], progress: true });
  });

  it("skips a commit that adds nothing to the base", () => {
    const result = { commits: 0, complete: true, checked: "at-base" };
    assert.deepEqual(mergeCandidates([result], ahead, none), { toMerge: [], progress: false });
  });

  it("skips incomplete or unchecked results", () => {
    const results = [
      { commits: 1, complete: false, checked: "abc" },
      { commits: 1, complete: true },
    ];
    assert.deepEqual(mergeCandidates(results, ahead, none), { toMerge: [], progress: true });
  });
});
