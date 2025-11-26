using System;

namespace Vidora.Core.Exceptions;

public abstract class CoreException : Exception
{
    public CoreException() { }

    public CoreException(string message)
        : base(message) { }

    public CoreException(string message, Exception innerException)
        : base(message, innerException) { }
}
