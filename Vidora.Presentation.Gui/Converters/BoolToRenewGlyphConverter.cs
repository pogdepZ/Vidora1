using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class BoolToRenewGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool hasActiveSubscription && hasActiveSubscription)
        {
            return "\uE72C"; // Refresh/Renew icon
        }
        return "\uE8FA"; // Add/Subscribe icon
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
