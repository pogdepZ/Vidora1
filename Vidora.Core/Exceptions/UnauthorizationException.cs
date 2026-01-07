namespace Vidora.Core.Exceptions;

public class UnauthorizationException : DomainException
{
    public UnauthorizationException() { }
    public UnauthorizationException(string message) : base(message) { }
}
