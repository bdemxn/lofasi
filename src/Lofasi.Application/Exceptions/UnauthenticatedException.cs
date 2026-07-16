namespace Lofasi.Application.Exceptions;

public sealed class UnauthenticatedException(string message) : BusinessException(message);
