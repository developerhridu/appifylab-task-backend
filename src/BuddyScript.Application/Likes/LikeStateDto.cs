namespace BuddyScript.Application.Likes;

/// <summary>Returned after a like/unlike so the client can reconcile optimistic UI.</summary>
public record LikeStateDto(int LikeCount, bool LikedByMe);
