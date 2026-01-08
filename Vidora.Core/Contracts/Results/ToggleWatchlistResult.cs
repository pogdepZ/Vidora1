namespace Vidora.Core.Contracts.Results;

public class ToggleWatchlistResult
{
    public bool IsInWatchlist { get; set; }
    public string Message { get; set; } = string.Empty;
}
