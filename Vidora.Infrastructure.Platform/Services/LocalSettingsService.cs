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
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                if (obj is string str)
                    return JsonSerializer.Deserialize<T>(str, options: JsonHelper.SerializerOptions);
            }
        }
        return default;
    }

    public void SaveSettings<T>(string key, T? value)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var json = JsonSerializer.Serialize(value, options: JsonHelper.SerializerOptions);
            ApplicationData.Current.LocalSettings.Values[key] = json;
        }
    }

    public void RemoveSettings(string key)
    {
        if (RuntimeHelper.IsMSIX)
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
        }
    }
}