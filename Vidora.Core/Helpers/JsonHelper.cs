using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vidora.Core.Helpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };


    //
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<T>(json, options ?? CamelCaseOptions);

    public static string Serialize(object obj, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(obj, options ?? CamelCaseOptions);

    //
    public static bool TryDeserialize<T>(string json, out T? value, JsonSerializerOptions? options = null)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(
                json,
                options ?? CamelCaseOptions
            );
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    public static bool TrySerialize<T>(T? value, out string? json, JsonSerializerOptions? options = null)
    {
        try
        {
            json = JsonSerializer.Serialize(
                value,
                options ?? CamelCaseOptions
            );
            return true;
        }
        catch (JsonException)
        {
            json = null;
            return false;
        }
    }
}
