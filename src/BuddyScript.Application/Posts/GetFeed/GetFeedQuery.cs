using MediatR;

namespace BuddyScript.Application.Posts.GetFeed;

public record GetFeedQuery(string? Cursor, int Limit = 10) : IRequest<FeedResult>;
