namespace Vidora.Core.Contracts.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string Username,
    string FullName
    );