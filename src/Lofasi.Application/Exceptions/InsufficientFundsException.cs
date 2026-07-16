namespace Lofasi.Application.Exceptions;

public sealed class InsufficientFundsException : BusinessException
{
    public InsufficientFundsException(string message)
        : base(message)
    {
    }
}
