namespace Vidora.Core.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException() { }
    public ValidationException(string message) : base(message) { }
}
