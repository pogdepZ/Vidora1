namespace Vidora.Core.Exceptions;

public sealed class ConnectionFailedException : DomainException
{
    public ConnectionFailedException() { }
    public ConnectionFailedException(string message) : base(message) { }
}
