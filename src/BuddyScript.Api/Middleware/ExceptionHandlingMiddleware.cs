using BuddyScript.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = BuddyScript.Application.Common.Exceptions.ValidationException;

namespace BuddyScript.Api.Middleware;

/// <summary>Translates Application-layer exceptions into RFC7807 problem responses.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed."),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden."),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError ? null : ex.Message
        };

        if (ex is ValidationException ve)
            problem.Extensions["errors"] = ve.Errors;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
