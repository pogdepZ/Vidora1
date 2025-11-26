using System;

namespace Vidora.Core.Contracts.Responses;

public record LoginResponse(
    string UserId,
    string Email,
    string Role,
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt
    );