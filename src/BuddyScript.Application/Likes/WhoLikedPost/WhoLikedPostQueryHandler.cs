using BuddyScript.Application.Common;
using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Likes.WhoLikedPost;

public class WhoLikedPostQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<WhoLikedPostQuery, IReadOnlyList<UserMini>>
{
    public async Task<IReadOnlyList<UserMini>> Handle(WhoLikedPostQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var canSee = await db.Posts.VisibleTo(userId).AnyAsync(p => p.Id == request.PostId, cancellationToken);
        if (!canSee) throw new NotFoundException("Post not found.");

        return await db.PostLikes
            .Where(l => l.PostId == request.PostId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new UserMini(l.User.Id, l.User.FirstName, l.User.LastName))
            .ToListAsync(cancellationToken);
    }
}
