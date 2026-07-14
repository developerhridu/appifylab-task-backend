using BuddyScript.Domain.Entities;

namespace BuddyScript.Application.Common.Interfaces;

public interface IJwtService
{
    /// <summary>Issues a signed JWT for the user; returns the token and its UTC expiry.</summary>
    (string Token, DateTimeOffset ExpiresAt) CreateToken(User user);
}
