using BuddyScript.Application.Common;
using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Comments.ListForPost;

public class ListCommentsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ListCommentsQuery, IReadOnlyList<CommentDto>>
{
    private record FlatComment(
        Guid Id, Guid AuthorId, string FirstName, string LastName,
        string Content, DateTimeOffset CreatedAt, int LikeCount, bool LikedByMe, Guid? ParentCommentId);

    public async Task<IReadOnlyList<CommentDto>> Handle(ListCommentsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var canSee = await db.Posts.VisibleTo(userId).AnyAsync(p => p.Id == request.PostId, cancellationToken);
        if (!canSee) throw new NotFoundException("Post not found.");

        // Single query: all comments for the post with counts + likedByMe. Tree assembled in memory.
        var flat = await db.Comments
            .Where(c => c.PostId == request.PostId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new FlatComment(
                c.Id, c.Author.Id, c.Author.FirstName, c.Author.LastName,
                c.Content, c.CreatedAt, c.Likes.Count, c.Likes.Any(l => l.UserId == userId), c.ParentCommentId))
            .ToListAsync(cancellationToken);

        var childrenByParent = flat
            .Where(c => c.ParentCommentId is not null)
            .ToLookup(c => c.ParentCommentId!.Value);

        CommentDto Build(FlatComment c) => new(
            c.Id,
            new UserMini(c.AuthorId, c.FirstName, c.LastName),
            c.Content,
            c.CreatedAt,
            c.LikeCount,
            c.LikedByMe,
            c.ParentCommentId,
            childrenByParent[c.Id].Select(Build).ToList());

        return flat.Where(c => c.ParentCommentId is null).Select(Build).ToList();
    }
}
