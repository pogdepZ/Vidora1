namespace Vidora.Infrastructure.Api.Options;

public class ApiOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
}