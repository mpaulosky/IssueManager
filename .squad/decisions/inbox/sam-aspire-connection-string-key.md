# Decision: Aspire Connection String Key Must Match Resource Name

**Author:** Sam (Backend Data Engineer)
**Date:** 2025-07-17
**Status:** Applied

## Context

The Api project used `GetConnectionString("IssueManagerDb")` but AppHost registers the MongoDB database as `AddDatabase("issuemanager")`. Aspire injects connection strings keyed by the resource name, so the Api was silently failing to pick up the Aspire-provided connection string and falling back to `mongodb://localhost:27017`.

## Decision

Changed `src/Api/Program.cs` line 15 from `"IssueManagerDb"` to `"issuemanager"` so the key matches what Aspire injects.

## Rule

Any service consuming an Aspire resource must use the exact resource name (case-sensitive) as the connection string key. The source of truth is the `AddDatabase()` / `AddConnectionString()` call in `src/AppHost/Program.cs`.
