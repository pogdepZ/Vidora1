using System;
using Vidora.Core.Entities; 

namespace Vidora.Core.Contracts.Commands;

public class UpdateProfileCommand
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public string? Birthday { get; set; }
    public string? Avatar { get; set; }
}