namespace Vidora.Core.Contracts.Commands;

public record UpdateProfileCommand(
    string Username,
    string FullName,
    string? Gender,      
    DateTime? Birthday,
    string? Avatar
);