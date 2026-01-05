namespace Vidora.Infrastructure.Api.Dtos.Requests;

internal record LoginRequest(
    string Email,
    string Password
);