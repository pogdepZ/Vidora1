using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class BoolToHeartGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isInWatchlist && isInWatchlist)
        {
            return "\uEB52"; // Filled heart
        }
        return "\uEB51"; // Empty heart
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
