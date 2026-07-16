namespace Lofasi.Application.Exceptions;

public sealed class ConflictException(string message) : BusinessException(message);
