using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Enums;

namespace BuddyScript.Application.Posts;

public record PostDto(
    Guid Id,
    UserMini Author,
    string Content,
    string? ImageUrl,
    Visibility Visibility,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByMe,
    int CommentCount);

/// <summary>One page of feed items plus the cursor to fetch the next page (null when exhausted).</summary>
public record FeedResult(IReadOnlyList<PostDto> Items, string? NextCursor);
