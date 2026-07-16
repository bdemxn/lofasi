namespace Lofasi.Application.Exceptions;

public sealed class ValidationException : BusinessException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
