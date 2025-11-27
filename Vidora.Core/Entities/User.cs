using Vidora.Core.ValueObjects;

namespace Vidora.Core.Entities;

public class User
{
    public required string Id { get; init; }
    public required Email Email { get; init; }
    public Role Role { get; init; } = Role.User;

    public bool IsAdmin => Role.Admin == Role;
}
