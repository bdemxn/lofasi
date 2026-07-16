namespace Lofasi.Application.Exceptions;

public sealed class NotFoundException(string message) : BusinessException(message);
