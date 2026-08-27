// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IEntity.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

namespace Shared.Abstractions;

/// <summary>
/// Shape every persisted MongoDB model shares: an identity and a soft-delete flag. Lets a generic
/// repository base filter by id and set the archived flag without knowing the concrete model type.
/// </summary>
public interface IEntity
{
	ObjectId Id { get; set; }

	bool Archived { get; set; }
}
