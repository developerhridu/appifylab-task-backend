using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Domain.Entities;
using Identity = Microsoft.AspNetCore.Identity;

namespace BuddyScript.Infrastructure.Auth;

/// <summary>Wraps ASP.NET Core Identity's PBKDF2 PasswordHasher (salted, versioned).</summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly Identity.PasswordHasher<User> _inner = new();
    private static readonly User Dummy = new();

    public string Hash(string password) => _inner.HashPassword(Dummy, password);

    public bool Verify(string passwordHash, string providedPassword)
    {
        var result = _inner.VerifyHashedPassword(Dummy, passwordHash, providedPassword);
        return result is Identity.PasswordVerificationResult.Success
            or Identity.PasswordVerificationResult.SuccessRehashNeeded;
    }
}
