namespace Lofasi.Application.Exceptions;

public sealed class InsufficientFundsException(string message) : BusinessException(message);
