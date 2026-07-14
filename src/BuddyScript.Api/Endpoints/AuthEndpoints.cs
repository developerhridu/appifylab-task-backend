using BuddyScript.Api.Auth;
using BuddyScript.Application.Auth;
using BuddyScript.Application.Auth.GetMe;
using BuddyScript.Application.Auth.Login;
using BuddyScript.Application.Auth.Register;
using BuddyScript.Application.DTOs;
using MediatR;

namespace BuddyScript.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender, HttpContext http, IWebHostEnvironment env) =>
        {
            var result = await sender.Send(command);
            SetAuthCookie(http, env, result);
            return Results.Created("/api/auth/me", result.User);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender, HttpContext http, IWebHostEnvironment env) =>
        {
            var result = await sender.Send(command);
            SetAuthCookie(http, env, result);
            return Results.Ok(result.User);
        });

        group.MapPost("/logout", (HttpContext http, IWebHostEnvironment env) =>
        {
            http.Response.Cookies.Delete(AuthCookie.Name, AuthCookie.BuildExpired(env.IsDevelopment()));
            return Results.NoContent();
        });

        group.MapGet("/me", async (ISender sender) =>
        {
            var user = await sender.Send(new GetMeQuery());
            return Results.Ok(user);
        }).RequireAuthorization();

        return app;
    }

    private static void SetAuthCookie(HttpContext http, IWebHostEnvironment env, AuthResult result) =>
        http.Response.Cookies.Append(AuthCookie.Name, result.Token, AuthCookie.Build(result.ExpiresAt, env.IsDevelopment()));
}
