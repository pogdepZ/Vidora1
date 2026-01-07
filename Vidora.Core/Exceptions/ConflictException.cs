namespace Vidora.Core.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException() { }
    public ConflictException(string message) : base(message) { }
}
