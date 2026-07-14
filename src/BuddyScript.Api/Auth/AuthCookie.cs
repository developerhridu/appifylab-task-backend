namespace BuddyScript.Api.Auth;

/// <summary>Centralizes the auth cookie name and hardened options.</summary>
public static class AuthCookie
{
    public const string Name = "access_token";

    public static CookieOptions Build(DateTimeOffset expiresAt, bool isDevelopment) => new()
    {
        HttpOnly = true,                         // not readable by JS → XSS-safe
        Secure = !isDevelopment,                 // HTTPS-only outside dev
        SameSite = SameSiteMode.Lax,             // sent on top-level navigations; blocks most CSRF
        Expires = expiresAt,
        Path = "/"
    };

    public static CookieOptions BuildExpired(bool isDevelopment) =>
        Build(DateTimeOffset.UtcNow.AddDays(-1), isDevelopment);
}
