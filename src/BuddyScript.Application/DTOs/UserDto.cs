namespace BuddyScript.Application.DTOs;

/// <summary>Minimal user projection embedded in posts/comments/likes.</summary>
public record UserMini(Guid Id, string FirstName, string LastName);

/// <summary>Authenticated user profile returned by /auth/me and after login/register.</summary>
public record UserDto(Guid Id, string FirstName, string LastName, string Email);
