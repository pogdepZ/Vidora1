using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class BooleanToBackButtonVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            if (Invert)
                b = !b;

            return b ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;
        }

        return NavigationViewBackButtonVisible.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is NavigationViewBackButtonVisible v)
        {
            bool b = v == NavigationViewBackButtonVisible.Visible;
            return Invert ? !b : b;
        }

        return false;
    }
}
