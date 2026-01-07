using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Vidora.Core.Exceptions;

namespace Vidora.Core.ValueObjects;

public sealed partial record Email
{
    private static readonly Regex EmailRegex = CreateEmailRegex();

    public string Value { get; init; }
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("Email cannot be empty.");

        if (!EmailRegex.IsMatch(value))
            throw new ValidationException("Invalid email format.");

        Value = value;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out Email? email, [NotNullWhen(false)] out string? error)
    {
        try
        {
            email = new Email(value);
            error = null;
            return true;
        }
        catch(ValidationException ex)
        {
            email = null;
            error = ex.Message;
            return false;
        }
    }

    public static implicit operator string(Email? email) => email?.Value ?? string.Empty;
    public static implicit operator Email(string value) => new Email(value);
    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateEmailRegex();

}