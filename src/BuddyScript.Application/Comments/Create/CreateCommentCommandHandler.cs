using BuddyScript.Application.Common;
using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Comments.Create;

public class CreateCommentCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var canSee = await db.Posts.VisibleTo(userId).AnyAsync(p => p.Id == request.PostId, cancellationToken);
        if (!canSee) throw new NotFoundException("Post not found.");

        // A reply's parent must exist and belong to the same post.
        if (request.ParentCommentId is not null)
        {
            var parentOk = await db.Comments.AnyAsync(
                c => c.Id == request.ParentCommentId && c.PostId == request.PostId, cancellationToken);
            if (!parentOk) throw new NotFoundException("Parent comment not found on this post.");
        }

        var comment = new Comment
        {
            PostId = request.PostId,
            AuthorId = userId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content.Trim()
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync(cancellationToken);

        var author = await db.Users.FindAsync([userId], cancellationToken);

        return new CommentDto(
            comment.Id,
            new UserMini(userId, author!.FirstName, author.LastName),
            comment.Content,
            comment.CreatedAt,
            LikeCount: 0,
            LikedByMe: false,
            comment.ParentCommentId,
            Replies: []);
    }
}
