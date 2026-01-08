using System;
using Vidora.Core.ValueObjects;

namespace Vidora.Core.Entities;

public class User
{
    public required int UserId { get; init; }
    public required Email Email { get; init; }
    public required string Username { get; set; }
    public required string FullName { get; set; }
    public Role Role { get; init; } = Role.User;
    public string? Avatar { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public Status? Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    public bool IsAdmin => Role.Admin == Role;
}
