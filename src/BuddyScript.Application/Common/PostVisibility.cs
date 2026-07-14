using BuddyScript.Domain.Entities;
using BuddyScript.Domain.Enums;

namespace BuddyScript.Application.Common;

public static class PostVisibility
{
    /// <summary>Posts the given user is allowed to see: all Public, plus their own Private.</summary>
    public static IQueryable<Post> VisibleTo(this IQueryable<Post> posts, Guid userId) =>
        posts.Where(p => p.Visibility == Visibility.Public
                         || (p.Visibility == Visibility.Private && p.AuthorId == userId));
}
