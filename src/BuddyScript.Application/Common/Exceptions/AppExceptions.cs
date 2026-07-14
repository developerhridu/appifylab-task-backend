namespace BuddyScript.Application.Common.Exceptions;

/// <summary>Input failed validation (400). Carries per-field errors.</summary>
public class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

/// <summary>Resource conflict, e.g. duplicate email (409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Authentication failed / not signed in (401).</summary>
public class UnauthorizedException(string message = "Unauthorized.") : Exception(message);

/// <summary>Authenticated but not allowed (403).</summary>
public class ForbiddenException(string message = "Forbidden.") : Exception(message);

/// <summary>Resource not found (404).</summary>
public class NotFoundException(string message = "Resource not found.") : Exception(message);
