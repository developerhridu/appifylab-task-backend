using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Domain.Entities;
using BuddyScript.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Likes.SetCommentLike;

public class SetCommentLikeCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<SetCommentLikeCommand, LikeStateDto>
{
    public async Task<LikeStateDto> Handle(SetCommentLikeCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        // Comment must belong to a post the user can see.
        var visible = await db.Comments
            .Where(c => c.Id == request.CommentId)
            .Select(c => c.Post.Visibility == Visibility.Public
                         || (c.Post.Visibility == Visibility.Private && c.Post.AuthorId == userId))
            .FirstOrDefaultAsync(cancellationToken);
        if (!visible) throw new NotFoundException("Comment not found.");

        var existing = await db.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == userId, cancellationToken);

        if (request.Like && existing is null)
            db.CommentLikes.Add(new CommentLike { CommentId = request.CommentId, UserId = userId });
        else if (!request.Like && existing is not null)
            db.CommentLikes.Remove(existing);

        await db.SaveChangesAsync(cancellationToken);

        var count = await db.CommentLikes.CountAsync(l => l.CommentId == request.CommentId, cancellationToken);
        return new LikeStateDto(count, request.Like);
    }
}
