namespace Lofasi.Application.Exceptions;

public sealed class UnauthenticatedException : BusinessException
{
    public UnauthenticatedException(string message)
        : base(message)
    {
    }
}
