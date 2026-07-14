using BuddyScript.Application.Likes;
using MediatR;

namespace BuddyScript.Application.Likes.SetCommentLike;

/// <summary>Like (Like=true) or unlike (Like=false) a comment or reply. Idempotent.</summary>
public record SetCommentLikeCommand(Guid CommentId, bool Like) : IRequest<LikeStateDto>;
