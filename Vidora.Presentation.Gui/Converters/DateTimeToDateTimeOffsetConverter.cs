using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    // Chuyển từ DateTime (Code) -> DateTimeOffset (Giao diện)
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
        {
            return new DateTimeOffset(date);
        }
        return null;
    }

    // Chuyển từ DateTimeOffset (Giao diện) -> DateTime (Code)
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dateOffset)
        {
            return dateOffset.DateTime;
        }
        return null;
    }
}