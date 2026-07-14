using MediatR;

namespace BuddyScript.Application.Comments.ListForPost;

public record ListCommentsQuery(Guid PostId) : IRequest<IReadOnlyList<CommentDto>>;
