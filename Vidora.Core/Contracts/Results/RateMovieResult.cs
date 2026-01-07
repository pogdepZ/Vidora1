namespace Vidora.Core.Contracts.Results;

public class RateMovieResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public double NewAvgRating { get; set; }
    public int UserRating { get; set; }
}