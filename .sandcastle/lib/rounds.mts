// What a round's results lead to: which checked commits go to the merger,
// and whether the round made progress or the run should stop.

// One issue pipeline's result: the commits it made this round, whether the
// implementer and reviewer completed, and the commit the host's check passed.
export type RoundResult = { commits: number; complete: boolean; checked?: string };

// A complete result goes to the merger when its checked commit adds to the
// base, even if this round made no commits (earlier work that failed the
// check then, on a flaky test say, and passes now), but never twice: a commit
// the merger was offered and didn't merge (a conflict it couldn't resolve)
// would fail the same way again. The round made progress when it committed
// something or has a commit to offer; otherwise the next plan would repeat it.
export function mergeCandidates<T extends RoundResult>(
  results: T[],
  aheadOfBase: (commit: string) => boolean,
  offered: ReadonlySet<string>,
): { toMerge: T[]; progress: boolean } {
  const toMerge = results.filter(
    (result) =>
      result.complete && result.checked !== undefined && !offered.has(result.checked) && aheadOfBase(result.checked),
  );
  return { toMerge, progress: toMerge.length > 0 || results.some((result) => result.commits > 0) };
}
