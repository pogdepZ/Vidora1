namespace Vidora.Core.Contracts.Requests;

public record LoginRequest(
    string Email,
    string Password
    );