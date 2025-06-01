using System.Globalization;
using System.Windows;
using System;
using System.Windows.Data;

namespace MakeGrid3D.Convertres
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue && visibilityValue == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }
}