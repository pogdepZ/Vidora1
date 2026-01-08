using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class BoolNegationConverter : IValueConverter
{
    // Chuyển đổi từ True -> False và ngược lại
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue;
        }
        return false;
    }

    // Chuyển đổi ngược (nếu cần TwoWay binding, dù thường ít dùng cho cái này)
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue;
        }
        return false;
    }
}