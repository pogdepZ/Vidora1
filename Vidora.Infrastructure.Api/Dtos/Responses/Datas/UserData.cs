using System;

namespace Vidora.Infrastructure.Api.Dtos.Responses.Datas;

internal record UserData(
    int UserId,
    string Email,
    string Username,
    string FullName,
    string Role,
    string? Avatar,
    string? Gender,
    DateTime? Birthday,
    string? Status,
    DateTime? CreatedAt
);
