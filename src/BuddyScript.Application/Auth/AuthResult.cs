using BuddyScript.Application.DTOs;

namespace BuddyScript.Application.Auth;

/// <summary>Returned by register/login handlers: token for the endpoint to set as a cookie, plus the user profile.</summary>
public record AuthResult(string Token, DateTimeOffset ExpiresAt, UserDto User);
