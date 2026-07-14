using MediatR;

namespace BuddyScript.Application.Likes.SetPostLike;

/// <summary>Like (Like=true) or unlike (Like=false) a post. Idempotent either way.</summary>
public record SetPostLikeCommand(Guid PostId, bool Like) : IRequest<LikeStateDto>;
