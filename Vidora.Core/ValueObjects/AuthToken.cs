using System;

namespace Vidora.Core.ValueObjects;

public record AuthToken(string Token, DateTime ExpiresAt)
{
    public AuthToken()
        : this(string.Empty, DateTime.UtcNow) { }

    public AuthToken(string token, TimeSpan expiresIn)
        : this(token, DateTime.UtcNow.Add(expiresIn)) { }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public string Scheme => "Bearer";
}