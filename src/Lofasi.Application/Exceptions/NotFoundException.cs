namespace Lofasi.Application.Exceptions;

public sealed class NotFoundException : BusinessException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
