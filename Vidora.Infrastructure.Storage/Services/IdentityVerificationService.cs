using System;
using System.Threading.Tasks;
using Vidora.Core.Interfaces.Storage;
using Windows.Security.Credentials.UI;

namespace Vidora.Infrastructure.Storage.Services;

public class IdentityVerificationService : IIdentityVerificationService
{
    public async Task<bool> IsAvailableAsync()
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        return availability == UserConsentVerifierAvailability.Available;
    }

    public async Task<bool> RequestConsentAsync(string message)
    {
        var result = await UserConsentVerifier.RequestVerificationAsync(message);
        return result == UserConsentVerificationResult.Verified;
    }
}
