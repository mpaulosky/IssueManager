# Squad Decisions

**Status:** ✅ Merged via squash to main (commit 676d76a)

---

### 2026-04-15: Soft delete architecture for Categories and Statuses
**By:** Matthew Paulosky (via Copilot)  
**What:** Soft delete (IsArchived flag) chosen over hard delete for Categories and Statuses. When a Category or Status is "deleted" by the user, it should be marked `IsArchived = true` rather than removed from the database. Issues associated with an archived Category/Status retain their association but the Category/Status is hidden from active selection UI.  
**Why:** User decision — unblocks issues previously labeled `go:needs-research` pending this architecture call.  
**Status:** ✅ Recorded — implementation in progress (Sprint 3)

---

### 2026-04-12: PR #113 — picomatch lockfile security bump
**By:** Legolas (Frontend)  
**What:** Merged PR #113 updating picomatch 4.0.3 → 4.0.4 in src/Web/package-lock.json. Security patch; CI validated; npm install clean.  
**Why:** Security fix for picomatch advisories; frontend-only scope; expected lockfile churn under Tailwind optional wasm entries was acceptable.  
**Status:** ✅ Merged to main; created reusable dependabot-lockfile-review skill
