using System;

namespace Vidora.Infrastructure.Api.Dtos.Requests;

internal record UpdateProfileRequest(
    string FullName,
    string Username,
    string? Gender,    
    String? Birthday,
    string? Avatar
);