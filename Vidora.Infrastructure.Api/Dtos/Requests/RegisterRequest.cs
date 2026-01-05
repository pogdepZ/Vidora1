namespace Vidora.Infrastructure.Api.Dtos.Requests;

internal record RegisterRequest(
    string Username,
    string FullName,
    string Email,
    string Password
    );
