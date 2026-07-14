using BuddyScript.Application.DTOs;
using MediatR;

namespace BuddyScript.Application.Likes.WhoLikedPost;

public record WhoLikedPostQuery(Guid PostId) : IRequest<IReadOnlyList<UserMini>>;
