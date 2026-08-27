// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IArchivableDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

namespace Shared.Abstractions;

/// <summary>
/// Shape every DTO shares for delete-by-archive: an identity and a soft-delete flag, both
/// read-only since DTOs are immutable records. Lets a generic delete handler check whether an
/// entity is already archived without knowing the concrete DTO type.
/// </summary>
public interface IArchivableDto
{
	ObjectId Id { get; }

	bool Archived { get; }
}
