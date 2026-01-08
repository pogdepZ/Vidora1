using System;

namespace Vidora.Core.Contracts.Results;

public record UserResult(
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