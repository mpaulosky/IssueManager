### 2026-03-06: Dirty Shared.Tests.Unit files — malformed copyright headers in working tree

**What:** After Ralph's session, 11 files in `tests/Shared.Tests.Unit/` have
corrupted copyright headers in the working tree (not committed). The closing
`// =============================================` line is missing and a partial
second header block is appended, causing `grep "^// File Name :"` to match
twice and produce a space-joined duplicate filename (e.g.,
`CategoryDtoTests.cs CategoryDtoTests.cs`).

**Why detected:** The pre-push hook uses a `||` fallback: if
`git diff --name-only HEAD @{push}` returns no `.cs` files (as happens with a
Scribe-only commit), it falls back to `git diff --name-only HEAD` (working
tree vs HEAD). The dirty `Shared.Tests.Unit` files are then checked and fail.

**Action required:** The next coding agent must inspect
`tests/Shared.Tests.Unit/**/*.cs`, fix the malformed headers (remove the
duplicate partial block), and commit the corrected files before pushing.

**Affected files (11):**
- tests/Shared.Tests.Unit/DTOs/CategoryDtoTests.cs
- tests/Shared.Tests.Unit/DTOs/CommentDtoTests.cs
- tests/Shared.Tests.Unit/DTOs/IssueDtoTests.cs
- tests/Shared.Tests.Unit/DTOs/LabelDtoTests.cs
- tests/Shared.Tests.Unit/DTOs/PaginatedResponseTests.cs
- tests/Shared.Tests.Unit/DTOs/StatusDtoTests.cs
- tests/Shared.Tests.Unit/DTOs/UserDtoTests.cs
- tests/Shared.Tests.Unit/Exceptions/ConflictExceptionTests.cs
- tests/Shared.Tests.Unit/Exceptions/NotFoundExceptionTests.cs
- tests/Shared.Tests.Unit/Helpers/HelpersTests.cs
- tests/Shared.Tests.Unit/Helpers/MyCategoriesTests.cs
