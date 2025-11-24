using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (Invert)
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility v)
        {
            bool b = v == Visibility.Visible;
            return Invert ? !b : b;
        }

        return false;
    }
}
