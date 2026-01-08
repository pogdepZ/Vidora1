using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace Vidora.Presentation.Gui.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("N0", new CultureInfo("vi-VN")) + " VNÐ";
        }
        if (value is double doubleValue)
        {
            return doubleValue.ToString("N0", new CultureInfo("vi-VN")) + " VNÐ";
        }
        if (value is int intValue)
        {
            return intValue.ToString("N0", new CultureInfo("vi-VN")) + " VNÐ";
        }
        return "0 VNÐ";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
