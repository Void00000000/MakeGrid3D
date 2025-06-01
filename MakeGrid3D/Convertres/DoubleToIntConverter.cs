using System;
using System.Globalization;
using System.Windows.Data;

namespace MakeGrid3D.Convertres
{
    public class DoubleToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i) return (double)i;
            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return (int)Math.Round(d);
            return 0;
        }
    }
}
