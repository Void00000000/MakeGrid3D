using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace MakeGrid3D.Convertres
{
    public class InvertedBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue && visibilityValue == Visibility.Visible)
            {
                return false;
            }
            return true;
        }
    }
}
