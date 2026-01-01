using System.Text.Json;
using Vidora.Core.Helpers;
using Windows.Storage;
using Vidora.Infrastructure.Platform.Helpers;
using Vidora.Core.Interfaces.Storage;

namespace Vidora.Infrastructure.Platform.Services;

public class LocalSettingsService : ILocalSettingsService
{
    public T? ReadSettings<T>(string key)
    {
        if (!RuntimeHelper.IsMSIX)
            return default;

        if (!ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            return default;

        if (obj is not string str)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(
                str,
                options: JsonHelper.SerializerOptions
            );
        }
        catch (JsonException)
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
            return default;
        }
    }

    public void SaveSettings<T>(string key, T? value)
    {
        if (!RuntimeHelper.IsMSIX)
            return;

        try
        {
            var json = JsonSerializer.Serialize(
                value,
                options: JsonHelper.SerializerOptions
            );

            ApplicationData.Current.LocalSettings.Values[key] = json;
        }
        catch (JsonException)
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
        }
    }

    public void RemoveSettings(string key)
    {
        if (!RuntimeHelper.IsMSIX)
            return;

        ApplicationData.Current.LocalSettings.Values.Remove(key);
    }
}