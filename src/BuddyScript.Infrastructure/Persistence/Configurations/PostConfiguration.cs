using BuddyScript.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuddyScript.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> b)
    {
        b.ToTable("posts");
        b.HasKey(p => p.Id);

        b.Property(p => p.Content).IsRequired();
        b.Property(p => p.ImagePath).HasMaxLength(512);
        b.Property(p => p.Visibility).HasConversion<int>().IsRequired();
        b.Property(p => p.CreatedAt).HasDefaultValueSql("now()");

        b.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Feed query: visibility filter + newest-first keyset on (CreatedAt, Id).
        b.HasIndex(p => new { p.Visibility, p.CreatedAt, p.Id })
            .HasDatabaseName("ix_posts_feed")
            .IsDescending(false, true, true);

        b.HasIndex(p => new { p.AuthorId, p.CreatedAt })
            .HasDatabaseName("ix_posts_author")
            .IsDescending(false, true);
    }
}
