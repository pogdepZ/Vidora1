using System;
using Vidora.Core.Entities; 

namespace Vidora.Core.Contracts.Commands;

public class UpdateProfileCommand
{
    public string FullName { get; set; }
    public string Username { get; set; }
    public Gender? Gender { get; set; }
    public String? Birthday { get; set; }
    public string? Avatar { get; set; }
}