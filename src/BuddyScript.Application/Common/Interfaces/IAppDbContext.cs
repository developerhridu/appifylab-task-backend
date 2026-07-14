using BuddyScript.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Common.Interfaces;

/// <summary>Persistence abstraction the Application layer depends on (implemented by Infrastructure).</summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<PostLike> PostLikes { get; }
    DbSet<CommentLike> CommentLikes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
