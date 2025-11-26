using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vidora.Core.Helpers;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<T>(json, options ?? SerializerOptions);

    public static string Serialize(object obj, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(obj, options ?? SerializerOptions);
}
