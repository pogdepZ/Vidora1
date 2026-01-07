namespace Vidora.Infrastructure.Api.Dtos.Requests;

internal record UpdateProfileRequest(
    string FullName,
    string Username,
    string? Gender,    
    string? Birthday,
    string? Avatar
);