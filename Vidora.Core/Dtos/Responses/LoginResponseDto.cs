using System;

namespace Vidora.Core.Dtos.Responses;

public record LoginResponseDto(
    string UserId,
    string Email,
    string Role,
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt
    );