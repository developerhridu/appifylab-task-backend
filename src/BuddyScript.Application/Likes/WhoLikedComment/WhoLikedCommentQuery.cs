using BuddyScript.Application.DTOs;
using MediatR;

namespace BuddyScript.Application.Likes.WhoLikedComment;

public record WhoLikedCommentQuery(Guid CommentId) : IRequest<IReadOnlyList<UserMini>>;
