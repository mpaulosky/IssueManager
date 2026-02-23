// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Abstractions;
using Shared.DTOs;
using Shared.Models;

namespace Api.Data;

/// <summary>
/// MongoDB implementation of the comment repository.
/// </summary>
public class CommentRepository : ICommentRepository
{
	private readonly IMongoCollection<Comment> _collection;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommentRepository"/> class.
	/// </summary>
	public CommentRepository(string connectionString, string databaseName = "IssueManagerDb")
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		_collection = database.GetCollection<Comment>("comments");
	}

	/// <inheritdoc />
	public async Task<Result> ArchiveAsync(Comment comment)
	{
		if (comment is null) return Result.Fail("Comment cannot be null.");

		comment.Archived = true;
		comment.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == comment.Id, comment);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Comment not found or could not be archived.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result> CreateAsync(Comment comment)
	{
		if (comment is null) return Result.Fail("Comment cannot be null.");

		await _collection.InsertOneAsync(comment);
		return Result.Ok();
	}

	/// <inheritdoc />
	public async Task<Result<Comment>> GetAsync(ObjectId itemId)
	{
		var entity = await _collection.Find(x => x.Id == itemId).FirstOrDefaultAsync();
		return entity is not null
			? Result.Ok(entity)
			: Result.Fail<Comment>("Comment not found.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<Comment>>> GetAllAsync()
	{
		var entities = await _collection.Find(_ => true).ToListAsync();
		return Result.Ok<IEnumerable<Comment>>(entities);
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<Comment>>> GetByUserAsync(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
			return Result.Fail<IEnumerable<Comment>>("User ID cannot be empty.");

		var entities = await _collection.Find(x => x.Author.Id == userId).ToListAsync();
		return Result.Ok<IEnumerable<Comment>>(entities);
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<Comment>>> GetByIssueAsync(IssueDto issue)
	{
		if (issue is null) return Result.Fail<IEnumerable<Comment>>("Issue cannot be null.");

		var entities = await _collection.Find(x => x.Issue.Id == issue.Id).ToListAsync();
		return Result.Ok<IEnumerable<Comment>>(entities);
	}

	/// <inheritdoc />
	public async Task<Result> UpdateAsync(ObjectId itemId, Comment comment)
	{
		if (comment is null) return Result.Fail("Comment cannot be null.");

		comment.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == itemId, comment);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Comment not found or could not be updated.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result> UpVoteAsync(ObjectId itemId, string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
			return Result.Fail("User ID cannot be empty.");

		var comment = await _collection.Find(x => x.Id == itemId).FirstOrDefaultAsync();
		if (comment is null)
			return Result.Fail("Comment not found.", ResultErrorCode.NotFound);

		if (!comment.UserVotes.Add(userId))
			return Result.Fail("User has already upvoted this comment.", ResultErrorCode.Conflict);

		comment.DateModified = DateTime.UtcNow;
		var result = await _collection.ReplaceOneAsync(x => x.Id == itemId, comment);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Comment could not be updated.", ResultErrorCode.NotFound);
	}
}
