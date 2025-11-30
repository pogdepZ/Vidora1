namespace Vidora.Core.Contracts.Requests;

public record RegisterRequest(
    string Email,
    string Password,
    string Username,
    string FullName
    );