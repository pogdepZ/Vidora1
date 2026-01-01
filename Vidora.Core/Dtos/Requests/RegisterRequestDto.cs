namespace Vidora.Core.Dtos.Requests;

public record RegisterRequestDto(
    string Email,
    string Password,
    string Username,
    string FullName
    );