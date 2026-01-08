using System;

namespace Vidora.Core.Extensions;

public static class EnumExtensions
{
    public static T ToEnum<T>(
        this string? value,
        T defaultValue,
        bool ignoreCase = true
        )
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<T>(value, ignoreCase, out var result)
            ? result
            : defaultValue;
    }

    public static T? ToEnum<T>(
        this string? value,
        bool ignoreCase = true)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase, out var result)
            ? result
            : null;
    }
}
