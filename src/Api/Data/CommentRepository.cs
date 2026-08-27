// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CommentRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

/// <summary>
/// MongoDB implementation of the comment repository.
/// </summary>
public class CommentRepository : MongoRepository<Comment, CommentDto>, ICommentRepository
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CommentRepository"/> class.
	/// </summary>
	public CommentRepository(string connectionString, string databaseName = "IssueManagerDb")
		: base(connectionString, databaseName, "comments", "Comment")
	{
	}

	/// <inheritdoc />
	protected override CommentDto ToDto(Comment model) => model.ToDto();

	/// <inheritdoc />
	protected override Comment ToModel(CommentDto dto) => dto.ToModel();

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<CommentDto>>> GetAllAsync(string? issueId = null, CancellationToken cancellationToken = default)
	{
		var filterBuilder = Builders<Comment>.Filter;
		var filter = filterBuilder.Empty;

		if (!string.IsNullOrWhiteSpace(issueId) && ObjectId.TryParse(issueId, out var objectId))
		{
			filter = filterBuilder.Eq(c => c.Issue.Id, objectId);
		}

		var entities = await Collection.Find(filter).ToListAsync(cancellationToken);
		return Result.Ok<IReadOnlyList<CommentDto>>(entities.Select(x => x.ToDto()).ToList().AsReadOnly());
	}

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<CommentDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default)
	{
		var filter = Builders<Comment>.Filter.Eq(x => x.Archived, false);
		var total = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await Collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		return entities.Count > 0
				? Result.Ok((entities.Select(x => x.ToDto()).ToList().AsReadOnly() as IReadOnlyList<CommentDto>, total))
				: Result.Fail<(IReadOnlyList<CommentDto> Items, long Total)>("Comments not found.");
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<CommentDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			return Result.Fail<IEnumerable<CommentDto>>("User ID cannot be empty.");
		}

		var entities = await Collection.Find(x => x.Author.Id == userId).ToListAsync(cancellationToken);

		return entities.Count > 0
				? Result.Ok(entities.Select(x => x.ToDto()).ToList().AsReadOnly() as IEnumerable<CommentDto>)
				: Result.Fail<IEnumerable<CommentDto>>("No comments found for the specified user.");
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<CommentDto>>> GetByIssueAsync(IssueDto issue, CancellationToken cancellationToken = default)
	{
		if (issue.Id == ObjectId.Empty)
			return Result.Fail<IEnumerable<CommentDto>>("Issue ID cannot be empty.");

		var entities = await Collection.Find(x => x.Issue.Id == issue.Id).ToListAsync(cancellationToken);

		return entities.Count > 0
				? Result.Ok(entities.Select(x => x.ToDto()).ToList().AsReadOnly() as IEnumerable<CommentDto>)
				: Result.Fail<IEnumerable<CommentDto>>("No comments found for the specified issue.");
	}

	/// <inheritdoc />
	public async Task<Result> UpVoteAsync(ObjectId itemId, string userId, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			return Result.Fail("User ID cannot be empty.");
		}

		var comment = await Collection.Find(x => x.Id == itemId).FirstOrDefaultAsync(cancellationToken);
		if (comment is null)
		{
			return Result.Fail("Comment not found.", ResultErrorCode.NotFound);
		}

		if (!comment.UserVotes.Add(userId))
		{
			return Result.Fail("User has already upvoted this comment.", ResultErrorCode.Conflict);
		}

		comment.DateModified = DateTime.UtcNow;
		var result = await Collection.ReplaceOneAsync(x => x.Id == itemId, comment, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Comment could not be updated.", ResultErrorCode.NotFound);
	}
}
