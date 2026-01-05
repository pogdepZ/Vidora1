namespace Vidora.Core.Contracts.Commands;

public record LoginCommand(
    string Email,
    string Password
    );
