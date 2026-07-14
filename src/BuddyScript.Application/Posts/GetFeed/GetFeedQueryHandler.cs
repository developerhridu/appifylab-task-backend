using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.Common.Pagination;
using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Posts.GetFeed;

public class GetFeedQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetFeedQuery, FeedResult>
{
    public async Task<FeedResult> Handle(GetFeedQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var limit = Math.Clamp(request.Limit, 1, 50);
        var cursor = FeedCursor.Decode(request.Cursor);

        var query = db.Posts.AsQueryable();

        // Visibility: everyone sees Public; only the author sees their own Private posts.
        query = query.Where(p => p.Visibility == Visibility.Public
                                 || (p.Visibility == Visibility.Private && p.AuthorId == userId));

        // Keyset: strictly older than the cursor on (CreatedAt, Id).
        if (cursor is not null)
        {
            query = query.Where(p =>
                p.CreatedAt < cursor.CreatedAt
                || (p.CreatedAt == cursor.CreatedAt && p.Id.CompareTo(cursor.Id) < 0));
        }

        var rows = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(limit + 1) // one extra row signals whether a next page exists
            .Select(p => new PostDto(
                p.Id,
                new UserMini(p.Author.Id, p.Author.FirstName, p.Author.LastName),
                p.Content,
                p.ImagePath,
                p.Visibility,
                p.CreatedAt,
                p.Likes.Count,
                p.Likes.Any(l => l.UserId == userId),
                p.Comments.Count))
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (rows.Count > limit)
        {
            var last = rows[limit - 1];
            nextCursor = new FeedCursor(last.CreatedAt, last.Id).Encode();
            rows.RemoveAt(rows.Count - 1);
        }

        return new FeedResult(rows, nextCursor);
    }
}
