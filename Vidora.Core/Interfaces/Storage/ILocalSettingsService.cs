namespace Vidora.Core.Interfaces.Storage;

public interface ILocalSettingsService
{
    T? ReadSettings<T>(string key);
    void SaveSettings<T>(string key, T? value);
    void RemoveSettings(string key);
}