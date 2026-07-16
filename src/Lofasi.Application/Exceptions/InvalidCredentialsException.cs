namespace Lofasi.Application.Exceptions;

public sealed class InvalidCredentialsException : BusinessException
{
    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
