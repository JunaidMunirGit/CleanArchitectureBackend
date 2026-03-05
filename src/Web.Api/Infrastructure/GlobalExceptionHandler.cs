using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Infrastructure;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        int statusCode = StatusCodes.Status500InternalServerError;
        string title = "Server failure";
        string? detail = null;

        // DbUpdateException (e.g. FK violation) from EF Core - return 400 so client gets a clear response
        if (exception.GetType().Name == "DbUpdateException")
        {
            statusCode = StatusCodes.Status400BadRequest;
            title = "Invalid data";
            detail = exception.InnerException?.Message ?? exception.Message;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = statusCode == 400
                ? "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
                : "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
