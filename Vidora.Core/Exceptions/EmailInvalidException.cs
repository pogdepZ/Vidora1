namespace Vidora.Core.Exceptions;

public sealed class EmailInvalidException : CoreException
{
    public EmailInvalidException() { }
    public EmailInvalidException(string message) : base(message) { }
}