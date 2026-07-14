using BuddyScript.Application.Common;
using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Likes.SetPostLike;

public class SetPostLikeCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<SetPostLikeCommand, LikeStateDto>
{
    public async Task<LikeStateDto> Handle(SetPostLikeCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        // Must be able to see the post to like it (404 hides private posts' existence).
        var canSee = await db.Posts.VisibleTo(userId).AnyAsync(p => p.Id == request.PostId, cancellationToken);
        if (!canSee) throw new NotFoundException("Post not found.");

        var existing = await db.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == request.PostId && l.UserId == userId, cancellationToken);

        if (request.Like && existing is null)
            db.PostLikes.Add(new PostLike { PostId = request.PostId, UserId = userId });
        else if (!request.Like && existing is not null)
            db.PostLikes.Remove(existing);

        await db.SaveChangesAsync(cancellationToken);

        var count = await db.PostLikes.CountAsync(l => l.PostId == request.PostId, cancellationToken);
        return new LikeStateDto(count, request.Like);
    }
}
