using BuddyScript.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuddyScript.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);

        b.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        b.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        b.Property(u => u.PasswordHash).IsRequired();

        // citext = case-insensitive; unique email regardless of casing.
        b.Property(u => u.Email).HasColumnType("citext").HasMaxLength(256).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();

        b.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
    }
}
