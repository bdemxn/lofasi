namespace Lofasi.Application.Abstractions.Services;

public interface IAccountNumberGenerator
{
    string Generate(DateOnly creationDate);
}
