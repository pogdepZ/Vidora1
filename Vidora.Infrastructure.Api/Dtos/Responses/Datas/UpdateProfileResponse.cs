using System;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

public record UpdateProfileResponse(
    int UserId,
    string FullName,
    string Username,
    string Email,
    string? Avatar,
    string Role,
    string Status,
    DateTime CreatedAt,
    string? Gender,
    string? PhoneNumber,
    DateTime? Birthday
);
