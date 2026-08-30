# IssueManager

An issue-tracking system: Issues are filed, classified, discussed, and resolved.

## Language

**Issue**:
The central record being tracked — a reported problem or piece of work, with a title, description, author, and lifecycle state.

**Taxonomy entity**:
A named, described reference entity that exists solely to classify Issues along one axis — Category and Status are the two Taxonomy entities today. A Taxonomy entity has no behavior of its own beyond being created, updated, listed, and archived.
_Avoid_: Lookup entity, reference entity, enum entity.

**Category**:
A Taxonomy entity classifying what kind of work an Issue represents.

**Status**:
A Taxonomy entity classifying an Issue's current lifecycle state.
