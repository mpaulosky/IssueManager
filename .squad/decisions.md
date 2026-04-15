# Squad Decisions

**Status:** ✅ Merged via squash to main (commit 676d76a)

---

### 2026-04-12: PR #113 — picomatch lockfile security bump
**By:** Legolas (Frontend)  
**What:** Merged PR #113 updating picomatch 4.0.3 → 4.0.4 in src/Web/package-lock.json. Security patch; CI validated; npm install clean.  
**Why:** Security fix for picomatch advisories; frontend-only scope; expected lockfile churn under Tailwind optional wasm entries was acceptable.  
**Status:** ✅ Merged to main; created reusable dependabot-lockfile-review skill
