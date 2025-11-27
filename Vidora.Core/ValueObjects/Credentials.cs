using System.Diagnostics.CodeAnalysis;

namespace Vidora.Core.ValueObjects;

public record Credentials(Email Email, Password Password)
{
    public static bool TryCreate(
        string emailValue,
        string passwordValue,
        [NotNullWhen(true)] out Credentials? credentials,
        [NotNullWhen(false)] out string? error)
    {
        if (!Email.TryCreate(emailValue, out var email, out var emailError))
        {
            credentials = null;
            error = emailError;
            return false;
        }

        if (!Password.TryCreate(passwordValue, out var password, out var passwordError))
        {
            credentials = null;
            error = passwordError;
            return false;
        }

        credentials = new Credentials(email, password);
        error = null;
        return true;
    }
}