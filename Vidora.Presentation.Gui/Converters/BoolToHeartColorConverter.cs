using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Vidora.Presentation.Gui.Converters;

public class BoolToHeartColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isInWatchlist && isInWatchlist)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 9, 20)); // #E50914 - Red
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
