using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vidora.Presentation.Gui.Converters;

public partial class ListStringToStringConverter : IValueConverter
{
    public string Separator { get; set; } = ", ";
    public int MaxItems { get; set; } = 3;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not List<string> list || list.Count == 0)
            return string.Empty;

        return string.Join(Separator, list.Take(MaxItems));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value;
}
