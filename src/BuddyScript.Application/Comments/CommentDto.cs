using BuddyScript.Application.DTOs;

namespace BuddyScript.Application.Comments;

public record CommentDto(
    Guid Id,
    UserMini Author,
    string Content,
    DateTimeOffset CreatedAt,
    int LikeCount,
    bool LikedByMe,
    Guid? ParentCommentId,
    IReadOnlyList<CommentDto> Replies);
