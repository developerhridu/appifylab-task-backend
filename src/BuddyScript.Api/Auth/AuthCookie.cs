namespace BuddyScript.Api.Auth;

/// <summary>Centralizes the auth cookie name and hardened options.</summary>
public static class AuthCookie
{
    public const string Name = "access_token";

    public static CookieOptions Build(DateTimeOffset expiresAt, bool isDevelopment) => new()
    {
        HttpOnly = true,                    // not readable by JS → XSS-safe
        // Dev: same-site over http (localhost). Prod: frontend and API live on
        // different domains (Vercel ↔ Render), so the cookie must be SameSite=None
        // to travel on cross-site fetches — which requires Secure (HTTPS).
        Secure = !isDevelopment,
        SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
        Expires = expiresAt,
        Path = "/"
    };

    public static CookieOptions BuildExpired(bool isDevelopment) =>
        Build(DateTimeOffset.UtcNow.AddDays(-1), isDevelopment);
}
