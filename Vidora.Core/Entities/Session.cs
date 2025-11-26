using Vidora.Core.ValueObjects;

namespace Vidora.Core.Entities;

public class Session
{
    public required User CurrentUser { get; set; }
    public required AuthToken AccessToken { get; set; }
    public required AuthToken RefreshToken { get; init; }
}
