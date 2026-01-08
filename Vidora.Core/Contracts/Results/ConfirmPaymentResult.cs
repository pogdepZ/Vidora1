namespace Vidora.Core.Contracts.Results;

public class ConfirmPaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
