using System.Threading.RateLimiting;
using BuddyScript.Api.Endpoints;
using BuddyScript.Api.Extensions;
using BuddyScript.Api.Middleware;
using BuddyScript.Application;
using BuddyScript.Infrastructure;
using BuddyScript.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiAuth(builder.Configuration);
builder.Services.AddApiCors(builder.Configuration);

// Throttle auth endpoints per client IP to blunt credential brute-forcing.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending EF Core migrations on startup. Single-instance deploy (Render),
// so no distributed migration lock needed. A failure here aborts boot, which
// fails the deploy and keeps the previous version live.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Render terminates TLS at its edge and forwards HTTP with X-Forwarded-Proto.
// Honor it so UseHttpsRedirection sees the original scheme and does not loop.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enforce HTTPS only in production. In dev the frontend is http://localhost, and
// redirecting the API to https would make it cross-scheme (schemeful same-site),
// which stops the SameSite=Lax auth cookie from being sent.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors(ApiServiceExtensions.CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapCommentEndpoints();
app.MapLikeEndpoints();

app.Run();
