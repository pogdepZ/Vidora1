using System.Diagnostics.CodeAnalysis;
using Vidora.Core.Exceptions;

namespace Vidora.Core.ValueObjects;

public sealed record Password
{
    public string Value { get; }
    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PasswordInvalidException("Password cannot be empty.");
        if (value.Length < 6)
            throw new PasswordInvalidException("Password too short.");

        Value = value;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out Password? password, [NotNullWhen(false)] out string? error)
    {
        try
        {
            password = new Password(value);
            error = null;
            return true;
        }
        catch (CoreException ex)
        {
            password = null;
            error = ex.Message;
            return false;
        }
    }

    public static implicit operator string(Password? p) => p?.Value ?? string.Empty;
    public static implicit operator Password(string value) => new Password(value);
    public override string ToString() => Value;
}