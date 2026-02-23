// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     User.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Models;

/// <summary>
///   User class
/// </summary>
[Serializable]
public class User
{
	/// <summary>
	///   Gets or sets the identifier.
	/// </summary>
	/// <value>
	///   The identifier.
	/// </value>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the name.
	/// </summary>
	/// <value>
	///   The name.
	/// </value>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the email address.
	/// </summary>
	/// <value>
	///   The email address.
	/// </value>
	public string Email { get; set; } = string.Empty;

	/// <summary>
	///   Gets or sets the date created.
	/// </summary>
	/// <value>
	///   The date created.
	/// </value>
	public DateTime DateCreated { get; init; } = DateTime.UtcNow;

	/// <summary>
	///   Gets or sets the date modified.
	/// </summary>
	/// <value>
	///   The date modified.
	/// </value>
	public DateTime? DateModified { get; set; }

}
