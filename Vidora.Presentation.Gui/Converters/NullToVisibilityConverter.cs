using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Vidora.Presentation.Gui.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isNull = value is null || (value is string s && string.IsNullOrEmpty(s));
        
        if (Invert)
        {
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return isNull ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
