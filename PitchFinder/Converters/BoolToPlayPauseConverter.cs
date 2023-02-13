using System;
using System.Globalization;
using System.Windows.Data;

namespace PitchFinder.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    class BoolToPlayPauseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "\uE769" : "\uE768";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
