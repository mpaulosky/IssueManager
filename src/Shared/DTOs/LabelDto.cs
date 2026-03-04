// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     LabelDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
namespace Shared.DTOs;

public record LabelDto(string Name, string Color)
{
	/// <summary>
	///   Gets an empty LabelDto instance.
	/// </summary>
	public static LabelDto Empty => new(string.Empty, string.Empty);
}