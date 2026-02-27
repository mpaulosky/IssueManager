# Web Interface + Auth0 + Theme Milestone

**Status:** Planning  
**Lead:** Aragorn  
**Security Review:** Gandalf  
**Scope:** Full Blazor CRUD UI for Issues, Categories, Comments, and Statuses; Auth0 authentication
and authorization; UserDto integration for logged-in user attribution; theme/color switcher fixes.

---

## Goals

1. All four entities (Issues, Categories, Comments, Statuses) are manageable through the UI
2. Auth0 authentication protects mutating operations; read-only browsing remains public
3. Logged-in user is recorded as `Author`/`ArchivedBy` on created and archived entities
4. Dark/light toggle and color theme switcher work correctly and persist across page loads
5. All new code has bUnit test coverage; build and tests remain green

---

## Work Streams

### Stream 1 — API Client Services (Foundation)

**Owner:** Sam | **Priority:** P0 | **Blocks:** Streams 2.6, 3, 4.9

| # | Item | Description |
|---|------|-------------|
| 1.1 | `IssueApiClient` | Typed HTTP client for `/api/v1/issues`: list (paginated), get by id, create, update, archive |
| 1.2 | `CategoryApiClient` | Typed HTTP client for `/api/v1/categories`: list, get by id, create, update, archive |
| 1.3 | `StatusApiClient` | Typed HTTP client for `/api/v1/statuses`: list, get by id, create, update, archive |
| 1.4 | `CommentApiClient` | Typed HTTP client for `/api/v1/comments`: list, get by id, create, update, archive |
| 1.5 | Register in DI | Register all four typed clients in `Web/Program.cs` |

---

### Stream 2 — Shared UI Components

**Owner:** Legolas | **Priority:** P0 | **Blocks:** Stream 3

| # | Item | Description |
|---|------|-------------|
| 2.1 | `DataTable<T>` | Generic table component: column definitions, empty state, theming |
| 2.2 | `Pagination` | Page prev/next/numbers; `OnPageChanged` callback |
| 2.3 | `ConfirmDialog` | Modal overlay; confirm/cancel; `OnConfirm` EventCallback |
| 2.4 | `StatusBadge` | Renders `StatusDto` as colored badge |
| 2.5 | `LoadingSpinner` | Reusable loading indicator |
| 2.6 | Extend `IssueForm` | Real Category/Status dropdowns from API; Author from auth context |

---

### Stream 3 — CRUD Pages

**Owner:** Legolas (markup) + Sam (code) | **Priority:** P1 | **Depends on:** Streams 1, 2

| # | Item | Route |
|---|------|-------|
| 3.1 | Issues list + search by author | `/issues` — includes filter bar: search by author name/email, filter by status, filter by category; results paginated |
| 3.2 | Issue create | `/issues/create` |
| 3.3 | Issue detail + comments | `/issues/{id}` |
| 3.4 | Issue edit | `/issues/{id}/edit` |
| 3.5 | Categories list | `/categories` |
| 3.6 | Category create | `/categories/create` |
| 3.7 | Category edit | `/categories/{id}/edit` |
| 3.8 | Statuses list | `/statuses` |
| 3.9 | Status create | `/statuses/create` |
| 3.10 | Status edit | `/statuses/{id}/edit` |
| 3.11 | Comments inline on issue detail | (within 3.3) |
| 3.12 | Comment create/edit/delete actions | (within 3.3) |
| 3.13 | Update `NavMenu.razor` | Add Issues, Categories, Statuses links |

---

### Stream 4 — Auth0 Authentication & Authorization

**Owner:** Gandalf (security) + Sam (backend) | **Priority:** P1

**Auth0 Flow:** Authorization Code + PKCE via `Auth0.AspNetCore.Authentication`.  
Tokens remain server-side (Blazor Server). API validates JWT Bearer tokens.

| # | Item | Description |
|---|------|-------------|
| 4.1 | NuGet packages | `Auth0.AspNetCore.Authentication`, `Microsoft.AspNetCore.Authentication.JwtBearer` in `Directory.Packages.props` |
| 4.2 | Web auth middleware | `AddAuth0WebAppAuthentication` in `Web/Program.cs`; `UseAuthentication` + `UseAuthorization`; `appsettings.json` Auth0 section |
| 4.3 | API JWT validation | `AddAuthentication().AddJwtBearer(...)` in `Api/Program.cs` |
| 4.4 | Login/Logout endpoints | `/Account/Login` and `/Account/Logout`; NavMenu login/logout buttons |
| 4.5 | `CurrentUserService` | Maps Auth0 claims (`sub`, `name`, `email`) to `UserDto`; scoped service |
| 4.6 | Wire UserDto | Author/ArchivedBy populated from `CurrentUserService` on create/archive |
| 4.7 | Authorization policies | Policies: `CanCreate`, `CanEdit`, `CanArchive`. Apply to mutating API endpoints |
| 4.8 | Protect Blazor routes | `[Authorize]` on create/edit pages; `AuthorizeView` in NavMenu |
| 4.9 | Token forwarding | `DelegatingHandler` attaches Bearer token to Web → API calls |

**RBAC Roles:**

| Role | Permissions |
|------|-------------|
| `admin` | Full access including delete and category/status management |
| `editor` | Create/update own Issues and Comments |
| `viewer` | Read-only |

---

### Stream 5 — Theme & Color Switcher

**Owner:** Legolas | **Priority:** P2

| # | Item | Description |
|---|------|-------------|
| 5.1 | Runtime verification | Test dark/light toggle + color switcher persistence; document findings |
| 5.2 | Fix identified issues | Likely: Flash of Unstyled Content (FOUC) from `<body>` IIFE placement; SSR icon flash; Blazor re-render reset |
| 5.3 | OS preference fallback | Default to `prefers-color-scheme` for first-time visitors |

---

### Stream 6 — Testing

**Owner:** Gimli | **Priority:** P1

| # | Item | Target |
|---|------|--------|
| 6.1 | API client bUnit tests | ≥1 test per client method (20+ tests) |
| 6.2 | Shared component bUnit tests | All 6 new components |
| 6.3 | CRUD page bUnit tests | ≥2 per page (30+ tests) |
| 6.4 | Auth integration tests | Protected routes, `CurrentUserService` claim mapping |
| 6.5 | Playwright E2E (stretch) | Login → create issue → edit → archive happy path |

---

## Dependency Graph

```
Stream 1 (API Clients) ─────────────────────────────────────────▶ Stream 3
  (start immediately)                                                   ▲
                                                                        │
Stream 2 (Shared Components 2.1–2.5) ──────────────────────────────────┤
  (start immediately)                                                   │
    └─▶ 2.6 (IssueForm) requires Stream 1 ─────────────────────────────┤
                                                                        │
Stream 4 (Auth0)                                                        │
  4.1 → 4.2 + 4.3 → 4.4 + 4.5 + 4.7 + 4.8 + 4.9 → 4.6 ─────────────┤
                                                                        │
Stream 5 (Theme) — independent, start anytime                          │
                                                                        │
Stream 6 (Testing) ◀────────────────────────────── follows each stream ┘
```

---

## Acceptance Criteria

The milestone closes when ALL of the following pass:

1. ☐ All entity list pages render live API data
2. ☐ Create, edit, and archive work for Issues, Categories, Statuses, Comments via the UI
3. ☐ Auth0 login/logout functional; NavMenu shows user name when authenticated
4. ☐ Unauthenticated users can browse but cannot create/edit/archive
5. ☐ Created and archived items record the authenticated user
6. ☐ Dark/light toggle and color theme switcher persist across page reloads
7. ☐ All new components and pages have bUnit tests
8. ☐ `dotnet build` → 0 errors; all tests pass
9. ☐ README updated with Auth0 setup instructions

---

## Security Gaps (from Gandalf's Audit)

Current state: **Auth0 NOT STARTED — all 20 API endpoints are publicly accessible.**

Critical risks that this milestone must resolve:

- 🔴 All CRUD endpoints unprotected — anyone can create/update/delete without authentication
- 🔴 No identity on mutations — Author field has no verified identity
- 🟡 No rate limiting on API endpoints
- 🟡 MongoDB fallback connection string (`mongodb://localhost:27017`) needs environment guard
- 🟠 Tailwind Play CDN loaded from external URL — supply chain risk; bundle with CLI build

---

## Auth0 Tenant Configuration Required

Before development begins, the Auth0 tenant needs:

1. **Application:** Regular Web Application (Authorization Code + PKCE)
2. **API:** Register audience `https://api.issuemanager.com`
3. **Permissions:** `read:issues`, `write:issues`, `delete:issues`,
   `manage:categories`, `manage:statuses`, `write:comments`, `delete:comments`
4. **Roles:** `admin`, `editor`, `viewer` with permissions mapped
5. **GitHub Secrets:** `AUTH0_DOMAIN`, `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET`, `AUTH0_AUDIENCE`

---

## Team Assignments

| Agent | Primary Streams | Notes |
|-------|-----------------|-------|
| Aragorn | Architecture review, PR gates | Unblocks decisions |
| Sam | Streams 1, 3 (`@code` blocks), 4.3/4.7 | Backend + API wiring |
| Legolas | Streams 2, 3 (markup), 5 | Frontend + theme |
| Gandalf | Stream 4 (auth/authz) | Security gate on all PRs |
| Gimli | Stream 6 | Test coverage enforcer |
| Frodo | README auth setup docs | Documentation |
