namespace Vidora.Core.Exceptions;

public sealed class PasswordInvalidException : CoreException
{
    public PasswordInvalidException() { }
    public PasswordInvalidException(string message) : base(message) { }
}