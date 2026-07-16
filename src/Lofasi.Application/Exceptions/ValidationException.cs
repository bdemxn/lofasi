namespace Lofasi.Application.Exceptions;

public sealed class ValidationException(string message) : BusinessException(message);
