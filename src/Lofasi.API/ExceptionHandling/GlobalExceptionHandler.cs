using Lofasi.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Lofasi.API.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            InsufficientFundsException => StatusCodes.Status400BadRequest,
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,
            UnauthenticatedException => StatusCodes.Status401Unauthorized,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            BusinessException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            logger.LogWarning(exception, "Business exception occurred.");
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            statusCode,
            statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            httpContext.TraceIdentifier);

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
