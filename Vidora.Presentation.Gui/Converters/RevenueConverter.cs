using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Vidora.Presentation.Gui.Converters
{
    public sealed class RevenueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var points = new PointCollection();

            var data = value as IEnumerable<double>;
            if (data == null) return points;

            var arr = data.ToArray();
            if (arr.Length == 0) return points;

            // Normalize X,Y về 0..100 để Polyline Stretch=Fill tự scale
            double min = arr.Min();
            double max = arr.Max();
            double range = max - min;
            if (range <= 0) range = 1;

            int n = arr.Length;
            for (int i = 0; i < n; i++)
            {
                double x = (n == 1) ? 0 : (i * 100.0 / (n - 1));
                double yNorm = (arr[i] - min) / range;     // 0..1
                double y = 100.0 - (yNorm * 100.0);        // đảo trục để “cao” = giá trị lớn

                points.Add(new Point(x, y));
            }

            return points;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotSupportedException();
    }
}
