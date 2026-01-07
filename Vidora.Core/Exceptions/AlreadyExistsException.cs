namespace Vidora.Core.Exceptions;

public sealed class AlreadyExistsException : DomainException
{
    public AlreadyExistsException() { }
    public AlreadyExistsException(string message) : base(message) { }
}
