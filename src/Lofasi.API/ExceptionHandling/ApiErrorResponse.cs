namespace Lofasi.API.ExceptionHandling;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Error,
    string TraceId);
