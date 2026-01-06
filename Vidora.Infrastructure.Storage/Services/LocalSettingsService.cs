using System.Text.Json;
using Vidora.Core.Helpers;
using Windows.Storage;
using Vidora.Core.Interfaces.Storage;
using Vidora.Infrastructure.Storage.Helpers;

namespace Vidora.Infrastructure.Storage.Services;

public class LocalSettingsService : ILocalSettingsService
{
    public T? ReadSettings<T>(string key)
    {
        if (!RuntimeHelper.IsMSIX)
            return default;

        if (!ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            return default;

        if (obj is not string json)
            return default;

        if (!JsonHelper.TryDeserialize<T>(json, out var value, options: JsonHelper.DefaultOptions))
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
            return default;
        }

        return value;
    }

    public void SaveSettings<T>(string key, T? value)
    {
        if (!RuntimeHelper.IsMSIX)
            return; 

        if (value is null)
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
            return;
        }

        if (!JsonHelper.TrySerialize(value, out var json, options: JsonHelper.DefaultOptions))
        {
            ApplicationData.Current.LocalSettings.Values.Remove(key);
            return;
        }

        ApplicationData.Current.LocalSettings.Values[key] = json;
    }

    public void RemoveSettings(string key)
    {
        if (!RuntimeHelper.IsMSIX)
            return;

        ApplicationData.Current.LocalSettings.Values.Remove(key);
    }
}