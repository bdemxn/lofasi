namespace Lofasi.Application.Exceptions;

public sealed class InvalidCredentialsException(string message) : BusinessException(message);
