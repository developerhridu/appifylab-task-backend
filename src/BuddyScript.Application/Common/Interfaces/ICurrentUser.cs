namespace BuddyScript.Application.Common.Interfaces;

/// <summary>Current authenticated user resolved from the request (null when anonymous).</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
