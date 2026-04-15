### 2026-04-15T02:46:00Z: User directive — Architecture decision
**By:** Matthew Paulosky (via Copilot)
**What:** Soft delete (IsArchived flag) chosen over hard delete for Categories and Statuses. When a Category or Status is "deleted" by the user, it should be marked IsArchived = true rather than removed from the database. Cascading behavior: Issues associated with an archived Category/Status retain their association but the Category/Status is hidden from active selection UI.
**Why:** User decision — unblocks issues previously labeled go:needs-research pending this architecture call.
