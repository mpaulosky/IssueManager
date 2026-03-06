# Session Log: Manual Changes Captured

**Timestamp:** 2026-03-06T19:48:59Z  
**Type:** Decision Capture & Logging  

## Summary

Two user directives were captured and logged:

1. **Web Project Restructured for Vertical Slice Architecture** — The `src/Web` project folder structure was manually reorganized to implement Vertical Slice Architecture. Features now live in self-contained slice folders rather than the previous horizontal layer-based structure.

2. **Test Project Renamed** — The test project was renamed from `Blazor.Tests` to `Web.Tests.Bunit` to better reflect that it tests the Web project specifically using bUnit.

Both decisions have been merged into `.squad/decisions.md` for team reference. All team members working on Web or test code should follow VSA conventions and use the correct test project name in future work.
