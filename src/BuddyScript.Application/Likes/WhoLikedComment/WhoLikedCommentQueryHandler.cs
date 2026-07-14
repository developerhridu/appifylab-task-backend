using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Likes.WhoLikedComment;

public class WhoLikedCommentQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<WhoLikedCommentQuery, IReadOnlyList<UserMini>>
{
    public async Task<IReadOnlyList<UserMini>> Handle(WhoLikedCommentQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var visible = await db.Comments
            .Where(c => c.Id == request.CommentId)
            .Select(c => c.Post.Visibility == Visibility.Public
                         || (c.Post.Visibility == Visibility.Private && c.Post.AuthorId == userId))
            .FirstOrDefaultAsync(cancellationToken);
        if (!visible) throw new NotFoundException("Comment not found.");

        return await db.CommentLikes
            .Where(l => l.CommentId == request.CommentId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new UserMini(l.User.Id, l.User.FirstName, l.User.LastName))
            .ToListAsync(cancellationToken);
    }
}
