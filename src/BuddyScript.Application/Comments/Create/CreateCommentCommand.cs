using MediatR;

namespace BuddyScript.Application.Comments.Create;

/// <summary>Create a comment, or a reply when ParentCommentId is set.</summary>
public record CreateCommentCommand(Guid PostId, string Content, Guid? ParentCommentId)
    : IRequest<CommentDto>;
