using BuddyScript.Domain.Common;
using BuddyScript.Domain.Enums;

namespace BuddyScript.Domain.Entities;

public class Post : BaseEntity
{
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Content { get; set; } = null!;
    public string? ImagePath { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}
