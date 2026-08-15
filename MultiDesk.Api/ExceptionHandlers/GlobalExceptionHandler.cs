using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MultiDesk.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            UnauthorizedAccessException ue => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ue.Message),

            InvalidOperationException ioe => (
                StatusCodes.Status409Conflict,
                "Conflict",
                ioe.Message),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not found",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception,
                "Unhandled exception (trace={TraceId})",
                httpContext.TraceIdentifier);

        var problem = new ProblemDetails
        {
            Status   = status,
            Title    = title,
            Detail   = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode      = status;
        httpContext.Response.ContentType     = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }
}